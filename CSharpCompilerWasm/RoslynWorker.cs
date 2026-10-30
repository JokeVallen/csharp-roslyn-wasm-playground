using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using CSharpCompilerWasm;

[SupportedOSPlatform("browser")]
public static partial class RoslynWorker
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private static readonly Dictionary<string, List<MetadataReference>> ReferenceCache = new();

    [JSExport]
    public static string GetCompletions(string code, int position, string targetFramework)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length > 30000)
            {
                return "[]";
            }

            var references = GetReferences(targetFramework);
            var tree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "CompletionAssembly",
                new[] { tree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, concurrentBuild: false));
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            position = Math.Clamp(position, 0, code.Length);
            var wordStart = position;
            while (wordStart > 0 &&
                   (char.IsLetterOrDigit(code[wordStart - 1]) || code[wordStart - 1] == '_'))
            {
                wordStart--;
            }

            var isMemberAccess = wordStart > 0 && code[wordStart - 1] == '.';
            var prefix = code[wordStart..position];
            if (prefix.Length == 0 && !isMemberAccess)
            {
                return "[]";
            }

            var suggestions = new HashSet<string>(StringComparer.Ordinal);
            if (isMemberAccess)
            {
                var token = root.FindToken(Math.Max(0, wordStart - 2));
                if (token.Parent is not null)
                {
                    var typeInfo = model.GetTypeInfo(token.Parent);
                    var type = typeInfo.Type ?? typeInfo.ConvertedType;
                    if (type is not null)
                    {
                        foreach (var member in type.GetMembers())
                        {
                            if (!member.IsImplicitlyDeclared && member.CanBeReferencedByName)
                            {
                                suggestions.Add(member.Name);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var symbol in model.LookupSymbols(position))
                {
                    if (symbol.CanBeReferencedByName)
                    {
                        suggestions.Add(symbol.Name);
                    }
                }
            }

            var result = suggestions
                .Where(item => prefix.Length == 0 || item.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                .Take(30)
                .ToArray();

            return JsonSerializer.Serialize(result, JsonOptions);
        }
        catch
        {
            return "[]";
        }
    }

    [JSExport]
    public static string Compile(string userCode, string targetFramework)
    {
        try
        {
            var code = BuildCompilationSource(userCode);
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation.Create(
                "DynamicAssembly",
                new[] { syntaxTree },
                GetReferences(targetFramework),
                new CSharpCompilationOptions(OutputKind.ConsoleApplication, concurrentBuild: false));

            using var stream = new MemoryStream();
            EmitResult emitResult = compilation.Emit(stream);
            if (emitResult.Success)
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    assemblyBase64 = Convert.ToBase64String(stream.ToArray()),
                    errors = Array.Empty<string>()
                }, JsonOptions);
            }

            var errors = emitResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d =>
                {
                    var line = d.Location.GetLineSpan().StartLinePosition.Line + 1;
                    return $"Line {line}: {d.GetMessage()}";
                })
                .Distinct()
                .Take(30)
                .ToArray();

            return JsonSerializer.Serialize(new
            {
                success = false,
                assemblyBase64 = (string?)null,
                errors
            }, JsonOptions);
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                assemblyBase64 = (string?)null,
                errors = new[] { ex.ToString() }
            }, JsonOptions);
        }
    }

    [JSExport]
    public static string Format(string code)
    {
        try
        {
            var root = CSharpSyntaxTree.ParseText(code).GetRoot();
            return root.NormalizeWhitespace().ToFullString();
        }
        catch
        {
            return code;
        }
    }

    [JSExport]
    public static string Execute(string assemblyBase64, string inputsJson)
    {
        string output = "";
        try
        {
            var inputs = JsonSerializer.Deserialize<string[]>(inputsJson) ?? Array.Empty<string>();
            var assembly = Assembly.Load(Convert.FromBase64String(assemblyBase64));
            var consoleType = assembly.GetType("Console");

            ClearConsole(consoleType);
            foreach (var input in inputs)
            {
                ProvideInput(consoleType, input);
            }

            var mainMethod = FindEntryPoint(assembly);
            if (mainMethod is null)
            {
                return SerializeRunResult("error", output, "No suitable Main method found.");
            }

            var parameters = mainMethod.GetParameters();
            object?[]? args = parameters.Length == 0 ? null : new object[] { Array.Empty<string>() };

            try
            {
                mainMethod.Invoke(null, args);
                output = GetConsoleOutput(consoleType);
                return SerializeRunResult("completed", output, null);
            }
            catch (TargetInvocationException ex) when (ex.InnerException?.Message == "INPUT_REQUIRED")
            {
                output = GetConsoleOutput(consoleType);
                return SerializeRunResult("inputRequired", output, null);
            }
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException ?? ex;
            return SerializeRunResult("error", output, $"{inner.GetType().Name}: {inner.Message}");
        }
    }

    private static MethodInfo? FindEntryPoint(Assembly assembly)
    {
        if (assembly.EntryPoint is not null)
        {
            return assembly.EntryPoint;
        }

        var programType = assembly.GetType("Program");
        if (programType is null)
        {
            return null;
        }

        return programType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public, null, Type.EmptyTypes, null)
            ?? programType.GetMethod("Main", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string[]) }, null);
    }

    private static void ClearConsole(Type? consoleType)
    {
        consoleType?.GetMethod("ClearOutput", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
    }

    private static void ProvideInput(Type? consoleType, string input)
    {
        consoleType?.GetMethod("ProvideInput", BindingFlags.Static | BindingFlags.NonPublic)
            ?.Invoke(null, new object[] { input });
    }

    private static string GetConsoleOutput(Type? consoleType)
    {
        return consoleType?.GetMethod("GetCapturedOutput", BindingFlags.Static | BindingFlags.NonPublic)
            ?.Invoke(null, null) as string ?? "";
    }

    private static string SerializeRunResult(string status, string output, string? error)
    {
        return JsonSerializer.Serialize(new
        {
            status,
            output,
            error
        }, JsonOptions);
    }

    private static string BuildCompilationSource(string code)
    {
        var lines = code.Replace("\r\n", "\n").Split('\n');
        var usingStatements = new List<string>();
        var userCodeLines = new List<string>();
        var inUsings = true;

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (inUsings && trimmed.StartsWith("using ", StringComparison.Ordinal))
            {
                usingStatements.Add(line);
            }
            else if (inUsings && (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("//", StringComparison.Ordinal)))
            {
                usingStatements.Add(line);
            }
            else
            {
                inUsings = false;
                userCodeLines.Add(line);
            }
        }

        return string.Join("\n", usingStatements) + "\n" +
               ConsoleCodeProvider.GetConsoleReplacementCode() + "\n" +
               string.Join("\n", userCodeLines);
    }

    private static List<MetadataReference> GetReferences(string targetFramework)
    {
        if (ReferenceCache.TryGetValue(targetFramework, out var cached))
        {
            return cached;
        }

        var references = new List<MetadataReference>();
        try
        {
            var referenceAssemblyType = GetReferenceAssemblyType(targetFramework);
            var referencesType = referenceAssemblyType.GetNestedType("References");
            if (referencesType is not null)
            {
                foreach (var property in referencesType.GetProperties(BindingFlags.Public | BindingFlags.Static))
                {
                    var value = property.GetValue(null);
                    if (value is MetadataReference reference)
                    {
                        references.Add(reference);
                    }
                    else if (value is IEnumerable<MetadataReference> collection)
                    {
                        references.AddRange(collection);
                    }
                }
            }
        }
        catch
        {
            // The caller receives an empty reference set and useful diagnostics.
        }

        ReferenceCache[targetFramework] = references;
        return references;
    }

    private static Type GetReferenceAssemblyType(string targetFramework)
    {
#if NET8_0
        return typeof(Basic.Reference.Assemblies.Net80);
#elif NET9_0
        return targetFramework == "net9.0"
            ? typeof(Basic.Reference.Assemblies.Net90)
            : typeof(Basic.Reference.Assemblies.Net80);
#else
        return targetFramework switch
        {
            "net8.0" => typeof(Basic.Reference.Assemblies.Net80),
            "net9.0" => typeof(Basic.Reference.Assemblies.Net90),
            _ => typeof(Basic.Reference.Assemblies.Net100)
        };
#endif
    }
}
