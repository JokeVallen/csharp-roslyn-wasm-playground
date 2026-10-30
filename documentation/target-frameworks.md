# Target framework selection

[简体中文](target-frameworks.zh-CN.md) | **English**

Each runtime build can compile user code against one or more reference-assembly sets (.NET 8, .NET 9, and/or .NET 10 depending on the build). This is a **compile-time target** switch, not a WebAssembly runtime switch.

## What changes

| Target | Reference package | Compile against |
| --- | --- | --- |
| `net9.0` (default) | `Basic.Reference.Assemblies.Net90` | .NET 9 APIs |
| `net8.0` | `Basic.Reference.Assemblies.Net80` | .NET 8 APIs |
| `net10.0` | `Basic.Reference.Assemblies.Net100` | .NET 10 APIs (available only in the .NET 10 runtime build) |

Each build runs on its own Blazor runtime: `/net8/` on .NET 8, `/net9/` on .NET 9, and `/net10/` on .NET 10. A compile target prevents Roslyn from using APIs outside the selected reference set, but the generated assembly is still executed by the runtime of the current page.

| Runtime build | Available compile targets |
| --- | --- |
| `/net8/` | `net8.0` |
| `/net9/` | `net8.0`, `net9.0` |
| `/net10/` | `net8.0`, `net9.0`, `net10.0` |

## How to select a target

### UI selector

The editor toolbar has a **目标 / Target** dropdown with `.NET 9` and `.NET 8`.

### URL parameter

```text
?target=net8.0
```

`target` is case-insensitive and also accepts `net8` as an alias. Unknown values fall back to `net9.0`.

Example:

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?target=net8.0&theme=light&accent=blue"
  width="100%" height="600" frameborder="0" loading="lazy"></iframe>
```

## Behavior notes

- The URL parameter sets the initial target.
- Changing the dropdown updates the current page immediately; it does not rewrite the iframe URL.
- Reloading the page restores the target from the URL (or `net9.0` if absent).
- Switching targets affects the next compilation, not code already running.
- Compile diagnostics may differ between targets because API availability differs.

## Important limitation

A single Blazor WebAssembly page cannot load two .NET runtimes at the same time. This feature does **not** switch the runtime in place.

For a real .NET 8 runtime, open `/net8/`; for .NET 10, open `/net10/`. The repository publishes all three builds. See [Runtime versions](runtimes.md).

## Implementation

| File | Change |
| --- | --- |
| `CSharpCompilerWasm/CSharpCompilerWasm.csproj` | References `Basic.Reference.Assemblies.Net80` and `Basic.Reference.Assemblies.Net90` |
| `CSharpCompilerWasm/Pages/Compiler.razor` | `target` query parameter, dropdown, target-specific reference type |
| `documentation/iframe-style-api.md` | Related embedding parameters |

The reference discovery still uses `typeof(MetadataReference).IsAssignableFrom(...)` because both packages expose `PortableExecutableReference` properties.
