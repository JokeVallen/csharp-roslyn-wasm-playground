# Browser C# runtime

[简体中文](browser-runtime.zh-CN.md) | **English**

This page describes how the playground compiles and executes C# entirely in the browser, what code shapes are supported, and where the implementation lives.

## Pipeline

1. The editor content is captured from the `<textarea>`.
2. In simplified mode, the content is indented and wrapped in:

   ```csharp
   using System;

   public class Program
   {
       public static void Main()
       {
           // editor content
       }
   }
   ```

3. `using` directives are extracted and placed at the top of the generated compilation unit.
4. A browser-friendly `Console` replacement is injected into the compilation.
5. The source and target framework are sent to the Roslyn Web Worker.
6. The worker loads the selected `Basic.Reference.Assemblies.Net80` / `Net90` / `Net100` reference set and creates a `CSharpCompilation`.
7. The worker emits the assembly to a `MemoryStream` and returns base64 bytes plus diagnostics.
8. The main thread forwards the base64 assembly to the worker's `RoslynWorker.Execute` export.
9. The worker loads the assembly with `Assembly.Load(byte[])`, finds `Main` (preferring `Assembly.EntryPoint`, then `Program.Main`), and invokes it.
10. `Console.ReadLine()` throws the internal `INPUT_REQUIRED` sentinel; the worker returns partial output, the main thread prompts the user, and the worker restarts execution with accumulated inputs.
11. An infinite or runaway program blocks only the worker; the console `Stop` button terminates the worker and recreates it on the next run.

## Supported code shape

The editor supports two modes:

- **Simplified mode** — write only the body of `Main`.
- **Full code mode** — write a complete C# program with `using` directives and a class containing `Main`.

The recommended entry point is:

```csharp
public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello");
    }
}
```

`Main()` and `Main(string[] args)` are recognized. The current runner invokes the method synchronously; do not rely on `async Task Main` being awaited.

## Roslyn references

`Basic.Reference.Assemblies.Net80`, `Net90`, and `Net100` contain reference assemblies as embedded resources. In `Compiler.razor` the code discovers members of the selected package's `References` nested type.

The `target` URL parameter or the editor toolbar selector chooses the compile target available in the current runtime build. This changes compile-time API availability only; the app still executes assemblies on the current page's WebAssembly runtime. See [Target framework selection](target-frameworks.md) and [Runtime versions](runtimes.md).

Important detail: the properties are `PortableExecutableReference`, not `MetadataReference` exactly. The fork uses:

```csharp
typeof(MetadataReference).IsAssignableFrom(p.PropertyType)
```

Using exact type equality causes zero references and compilation errors.

The project must also keep:

```xml
<WasmEnableWebcil>false</WasmEnableWebcil>
```

so the published `_framework` output keeps standard `.dll` files available for inspection and tooling.

## Editor assistance

- Typing triggers an immediate local keyword/member popup after 120 ms; it does not run Roslyn on every keystroke.
- Semantic completion is automatic by default after 420 ms of idle typing; switch `completion=manual` or use the `⚙ 设置` panel to require `Ctrl + Space` / the `✨ 提示` button.
- The semantic request is sent to `worker.js`, which boots the same .NET WebAssembly runtime and calls the exported `RoslynWorker.GetCompletions` method.
- The main thread stays responsive because Roslyn compilation, semantic completion, and user-code execution all run in the worker. The main thread only handles editor events, console output, input prompts, and UI animation.
- `Enter` auto-indents the new line from the current line's leading whitespace and adds one level after `{`, `(`, or `[`.
- Typing `}` on a whitespace-only line removes one indentation level.
- Pasting multiline text re-indents it relative to the current line.
- The `回车后格式化` setting formats the whole code with Roslyn after `Enter` (off by default).
- `ArrowUp` / `ArrowDown` move through suggestions and `Escape` closes the popup.
- When the popup is open, `Enter` or `Tab` accepts the selected suggestion. When the popup is closed, `Tab` inserts four spaces and `Shift + Tab` outdents the current line or selection.
- Local snippet completion for `for`, `foreach`, `if`, `while`, `do`, `switch`, and `try` shows a side-by-side multiline preview in the popup. Inserted multiline snippets inherit the current line's indentation. Use the item `<` / `>` button to collapse or expand the preview; drag the separator to resize the menu and the right edge to resize the preview.
- `Ctrl + Shift + F` formats the current code with Roslyn `NormalizeWhitespace()` inside the worker.
- The `⚙ 设置` panel contains the compile target, completion/format toggles, Enter/dedent/paste indentation toggles, code-snippet and preview toggles, configurable local/semantic/format delays, shortcut recording, idle worker prewarm, and the manual `✨ 提示` / `🧹 格式化` actions.
- Click outside the settings popover or press `Escape` to close it.
- Run, format, completion, and clear-console shortcuts can be recorded in the settings panel; defaults are `Ctrl + Enter`, `Ctrl + Shift + F`, `Ctrl + Space`, and `Ctrl + K`.
- `?code=base64url` prefills code and `readonly=1` disables editing.
- Trusted parent pages can run code through the browser-only `postMessage` API (`csharp-playground:run` / `csharp-playground:result`).
- The header toolbar can switch dark/light/system theme and accent color without reloading.
- The Run button shows an animated waiting state while compilation and execution are in progress.

## Keyboard shortcuts

| Shortcut | Action |
| --- | --- |
| `Ctrl + Enter` | Compile and run |
| `Ctrl + L` | Load sample code |
| `Ctrl + Space` | Request semantic completion |
| `Ctrl + Shift + F` | Format code |
| `Ctrl + K` | Clear console |

## Input handling

`Console.ReadLine()` does not block a browser thread. The runner executes the program with the inputs collected so far; when the injected `Console` throws the internal `INPUT_REQUIRED` marker, the UI asks for another line through JS interop and restarts the program with the complete input queue.

This means:

- input is collected interactively, but the program may execute from the beginning multiple times;
- side effects before the next `ReadLine()` can repeat;
- the final output shown is the output from the successful run.

## Limitations

- No real blocking console I/O in the browser.
- Only the `Console` subset documented in [Console API reference](console-api.md) is supported.
- No arbitrary NuGet packages; available references are the reference assemblies packaged with the app.
- No server-side filesystem, process, network, or OS APIs beyond what the browser runtime provides.
- Long-running or infinite loops can block the UI thread.
- The playground is an educational tool, not a security sandbox.

## Implementation locations

| File | Responsibility |
| --- | --- |
| `CSharpCompilerWasm/Pages/Compiler.razor` | Editor, compilation, emit, execution loop, diagnostics, UI parameters |
| `CSharpCompilerWasm/ConsoleReplacementCode.cs` | Source of the injected `Console` class |
| `CSharpCompilerWasm/CSharpCompilerWasm.csproj` | Runtime and Roslyn package versions, Webcil setting |
| `CSharpCompilerWasm/wwwroot/index.html` | Blazor host page and `base href` |
