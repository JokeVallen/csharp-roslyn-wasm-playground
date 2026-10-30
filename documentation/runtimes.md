# Runtime versions

[简体中文](runtimes.zh-CN.md) | **English**

The playground can be deployed as three real Blazor WebAssembly runtime builds:

| Runtime | URL | Blazor WASM package | Available compile targets |
| --- | --- | --- | --- |
| .NET 8 | `/net8/` | 8.0.33 | `net8.0` |
| .NET 9 | `/net9/` | 9.0.22 | `net8.0`, `net9.0` |
| .NET 10 | `/net10/` | 10.0.14 | `net8.0`, `net9.0`, `net10.0` |

The site root serves the .NET 10 build by default so existing links keep working.

## Choosing a runtime

### Direct URL

```text
https://csharp-roslyn-wasm-playground.pages.dev/net8/
https://csharp-roslyn-wasm-playground.pages.dev/net9/
https://csharp-roslyn-wasm-playground.pages.dev/net10/
```

### iframe

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/net8/?theme=light&accent=blue"
  width="100%" height="600" frameborder="0" loading="lazy"
  title="C# Playground on .NET 8"></iframe>
```

### `runtime` redirect parameter

The root page also accepts a `runtime` query parameter and redirects to the corresponding build:

```text
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net8.0
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net9.0
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net10.0
```

This is a navigation redirect, not an in-page runtime switch. A loaded Blazor WebAssembly page cannot replace its bundled `dotnet.wasm`.

## Why separate builds are required

Blazor WebAssembly bundles the runtime (`dotnet.wasm`) and boot configuration at publish time. The runtime is not a replaceable script selected from a dropdown.

Each runtime therefore needs its own `dotnet publish` output:

```bash
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net8.0 -o publish/net8
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net9.0 -o publish/net9
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net10.0 -o publish/net10
```

The repository's multi-runtime helper does this and rewrites the base href for each output:

```bash
./scripts/publish-all.sh /csharp-playground/ ./publish/site
# Windows:
./scripts/publish-all.ps1 -RootBaseHref /csharp-playground/ -OutputDir ./publish/site
```

## Local development

Install the SDKs you want to test, then select a target framework:

```bash
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net8.0
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net9.0
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net10.0
```

The launch profile URL remains `http://localhost:5158` unless changed in `Properties/launchSettings.json`.

## Deployment layout

The GitHub Pages artifact is assembled as:

```text
/
  index.html            # default .NET 10 build, base href /
  _framework/
  net8/
    index.html          # base href /csharp-playground/net8/
    _framework/
  net9/
    index.html          # base href /csharp-playground/net9/
    _framework/
  net10/
    index.html          # base href /csharp-playground/net10/
    _framework/
  .nojekyll
```

Each runtime has an independent payload and cache namespace. Users only download the runtime they open.

## Trade-offs

- Three publish outputs increase CI time and storage.
- The .NET 8 build can only target .NET 8 reference assemblies.
- The .NET 9 build can target .NET 8 and .NET 9.
- The .NET 10 build can target .NET 8, .NET 9, and .NET 10.
- Older .NET 6/7 are EOL and are intentionally not part of the matrix.

See [Target framework selection](target-frameworks.md) for the compile-target behavior inside each runtime build.
