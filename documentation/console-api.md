# Console API reference

[简体中文](console-api.zh-CN.md) | **English**

The playground replaces `System.Console` with a minimal `Console` class injected at compile time. It captures output and supports interactive line input through the browser UI.

## Supported members

### `Console.Write`

| Signature | Notes |
| --- | --- |
| `Write(string value)` | Writes text; `null` becomes an empty string. |
| `Write(char value)` | Writes one character. |
| `Write(object value)` | Writes `value?.ToString()`. |
| `Write(int value)` | Writes the integer. |
| `Write(long value)` | Writes the integer. |
| `Write(float value)` | Writes using invariant current culture formatting. |
| `Write(double value)` | Writes using current culture formatting. |
| `Write(decimal value)` | Writes using current culture formatting. |
| `Write(bool value)` | Writes `True` or `False`. |

### `Console.WriteLine`

| Signature | Notes |
| --- | --- |
| `WriteLine()` | Writes a newline. |
| `WriteLine(string value)` | Writes text followed by a newline. |
| `WriteLine(char value)` | Writes one character followed by a newline. |
| `WriteLine(object value)` | Writes `value?.ToString()` followed by a newline. |
| `WriteLine(int value)` | Writes the integer followed by a newline. |
| `WriteLine(long value)` | Writes the integer followed by a newline. |
| `WriteLine(float value)` | Writes the value followed by a newline. |
| `WriteLine(double value)` | Writes the value followed by a newline. |
| `WriteLine(decimal value)` | Writes the value followed by a newline. |
| `WriteLine(bool value)` | Writes `True` or `False` followed by a newline. |

### `Console.ReadLine`

```csharp
string name = Console.ReadLine();
```

Returns the next line entered by the user in the playground console.

Implementation detail: when no input is queued, it throws `InvalidOperationException("INPUT_REQUIRED")`. The runner catches that marker, asks the UI for another line, and restarts execution with the collected inputs. This is why the program may appear to run from the beginning again after input is entered.

## Not supported

The injected class is intentionally small. The following common `System.Console` members are **not** available:

- `Read()`, `ReadKey()`, `ReadLineAsync()`
- `WriteLine(string format, params object[] args)` overloads
- `ForegroundColor`, `BackgroundColor`, `ResetColor()`
- `Out`, `Error`, `In`
- `OpenStandardInput`, `SetOut`, `SetError`
- cursor positioning and other terminal APIs

String interpolation still works because it produces a string before calling `WriteLine`:

```csharp
Console.WriteLine($"Hello, {name}!");
```

## Examples

### Basic output

```csharp
Console.WriteLine("Hello");
Console.Write("C#");
Console.WriteLine(" Playground");
```

Output:

```text
Hello
C# Playground
```

### Interactive input

```csharp
Console.Write("What is your name? ");
string name = Console.ReadLine();
Console.WriteLine($"Hello, {name}!");
```

The first run reaches `ReadLine()`, the UI asks for input, and the program restarts with that input queued.

### Loops and calculations

```csharp
for (int i = 1; i <= 3; i++)
{
    Console.WriteLine($"Count: {i}");
}

Console.Write("Enter a number: ");
string input = Console.ReadLine();
if (int.TryParse(input, out int number))
{
    Console.WriteLine(number * 2);
}
```

## Internal host members

These are used by the Blazor runner and are intentionally not part of the user-facing API:

- `GetCapturedOutput()`
- `ClearOutput()`
- `ProvideInput(string input)`

## Source location

The generated class lives in:

`CSharpCompilerWasm/ConsoleReplacementCode.cs`

See [Browser C# runtime](browser-runtime.md) for the full execution pipeline and known limitations.
