# Differences from the upstream project

[简体中文](MODIFICATIONS.zh-CN.md) | **English**

This document explains what this repository changes, fixes, or adds compared with the upstream project [MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor). It is not a full changelog; upstream history and releases remain in the upstream repository.

**Upstream baseline used for this fork:** `MicrosoftLearning/c-sharp-minor` `main`, commit `dad9656533d764c0272f3fa2a986ca00de28aaca`.

## 1. Summary

| Area | Upstream | This fork | Why |
| --- | --- | --- | --- |
| Build output | Default Blazor Webcil output | `<WasmEnableWebcil>false</WasmEnableWebcil>` | Keep standard `.dll` files in `_framework` for browser-side tooling and troubleshooting |
| Roslyn reference discovery | Exact `PropertyType == typeof(MetadataReference)` check | `typeof(MetadataReference).IsAssignableFrom(...)` | `Basic.Reference.Assemblies.Net90` exposes `PortableExecutableReference`; the upstream check loaded zero references |
| Compile target | Fixed .NET 9 reference set | Selectable .NET 8 / .NET 9 / .NET 10 reference sets | Let users compile against the API surface of the selected framework |
| Runtime deployments | Single .NET 9 build | Real .NET 8, .NET 9, and .NET 10 builds under `/net8/`, `/net9/`, `/net10/` | Let iframe URLs and links choose the actual Blazor runtime |
| Live appearance | URL parameters only | Header theme and accent selectors | Change theme/accent without reloading or rebuilding |
| Editor assistance | Plain textarea | Semantic C# completion popup | Uses `SemanticModel.LookupSymbols` and member lookup while typing |
| Work execution | Roslyn and user code on the UI thread | Roslyn compilation, semantic completion, and user-code execution in a dedicated .NET Web Worker, with a Stop button | Keep typing and scrolling responsive and isolate runaway programs |
| Run feedback | Static button | Animated waiting state with a minimum visible duration | Make compilation/execution status obvious |
| UI | Single-column educational layout | Responsive two-pane workspace with themeable cards | Better readability and iframe embedding |
| Theming | Fixed Bootstrap/inline colors | URL-driven themes and presets | Different blog posts can use different looks without rebuilding |
| Deployment | Upstream GitHub Pages path `/c-sharp-minor/` | Fork-specific path `/csharp-roslyn-wasm-playground/` plus helper scripts | Deploy this fork as an independent site |
| Documentation | Upstream English README/security policy | Bilingual README, security policy, and this differences document | Clear fork identity and Chinese/English access |

## 2. Build and WebAssembly runtime

### `CSharpCompilerWasm/CSharpCompilerWasm.csproj`

Added:

```xml
<WasmEnableWebcil>false</WasmEnableWebcil>
```

Reason: this project expects standard `.dll` files to remain available in the published `_framework` directory. Disabling Webcil makes the browser-side output easier to inspect and avoids mixing Webcil packaging assumptions with the Roslyn/tooling workflow.

Everything else in the project file remains aligned with upstream: .NET 9, Roslyn 4.14.0, and `Basic.Reference.Assemblies.Net90`.

## 3. Roslyn metadata reference fix

### `CSharpCompilerWasm/Pages/Compiler.razor`

Upstream tries to discover reference assemblies via reflection:

```csharp
.Where(p => p.PropertyType == typeof(MetadataReference))
```

However, `Basic.Reference.Assemblies.Net90.References` exposes properties of type `PortableExecutableReference`, which derives from `MetadataReference`. Exact type equality therefore finds zero references and compilation fails.

This fork changed the check to:

```csharp
.Where(p => typeof(MetadataReference).IsAssignableFrom(p.PropertyType))
```

The playground also keeps diagnostic output that reports how many references were loaded, which makes this class of failure easier to spot.

### Selectable compile target

The fork adds `Basic.Reference.Assemblies.Net80` alongside `Net90`. The editor toolbar and the `target` URL parameter select which reference set Roslyn uses (`net9.0` by default, `net8.0` optional).

This is explicitly a **compile-time reference switch**, not a runtime switch: the Blazor app still runs on .NET 9. See [documentation/target-frameworks.md](documentation/target-frameworks.md).

## 4. UI redesign and iframe theming

### New stylesheet

`CSharpCompilerWasm/wwwroot/css/playground.css` is new and contains the fork's complete visual layer:

- dark, light, and `auto` themes;
- two-pane editor/console workspace;
- stacked layout for narrow screens;
- compact density mode;
- accessible focus states and reduced-motion handling;
- iframe-friendly embedded presentation.

### Razor markup and code

`CSharpCompilerWasm/Pages/Compiler.razor` now:

- adds `app-shell`, theme, layout, chrome, and density classes to the root container;
- reads iframe/URL style options from the query string;
- exposes `--accent`, `--accent-rgb`, `--radius`, and `--editor-font-size` CSS variables;
- renames the page title and header copy to “C# Playground”;
- gives the run button a visible “运行代码” label;
- updates console input styling to use theme CSS classes instead of hardcoded inline colors.

Supported URL parameters:

| Parameter | Values | Default |
| --- | --- | --- |
| `theme` | `dark`, `light`, `auto` | `dark` |
| `accent` | `indigo`, `blue`, `emerald`, `amber`, `rose`, `violet`, `cyan`, `orange`, or 6-digit hex without `#` | `indigo` |
| `layout` | `split`, `stacked` | `split` |
| `chrome` | `full`, `minimal`, `none` | `full` |
| `density` | `comfortable`, `compact` | `comfortable` |
| `radius` | `0`–`32` | `16` |
| `fontsize` | `11`–`22` | `14` |

Example:

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=emerald&density=compact&chrome=minimal"
  width="100%" height="560" frameborder="0" loading="lazy"></iframe>
```

### Host page

`CSharpCompilerWasm/wwwroot/index.html` now loads `css/playground.css` and uses the title `C# Playground`.

## 5. Deployment and hosting

The fork no longer targets the upstream `/c-sharp-minor/` path. It targets:

`/csharp-roslyn-wasm-playground/`

The deployment contains three real runtime builds: `/net8/`, `/net9/`, and `/net10/`. The site root serves the .NET 10 build for backward compatibility. See [documentation/runtimes.md](documentation/runtimes.md).

Changes include:

- a fork-specific `.github/workflows/deploy.yml` that publishes three framework builds and rewrites each base href;
- `scripts/publish.sh` and `scripts/publish.ps1` for self-hosted deployments;
- `deploy/nginx.conf.example` with SPA fallback and WASM/DLL MIME types;
- `.nojekyll` handling for GitHub Pages;
- iframe compatibility guidance: do not send `X-Frame-Options: SAMEORIGIN` or `DENY` for the playground response.

See [README.md](README.md) for build and deployment commands.

## 6. Documentation and attribution

This fork adds:

- `README.md` (English) and `README.zh-CN.md` (Chinese), with language links at the top;
- `SECURITY.md` (English) and `SECURITY.zh-CN.md` (Chinese);
- this bilingual differences document.

The documentation explicitly identifies the project as a community fork and retains the upstream MIT license and Microsoft copyright notice in [LICENSE](LICENSE).

## 7. Synchronizing with upstream

To bring in future upstream changes:

```bash
git remote add upstream https://github.com/MicrosoftLearning/c-sharp-minor.git
git fetch upstream
git merge upstream/main
```

Expect conflicts in:

- `CSharpCompilerWasm/Pages/Compiler.razor` (UI and reference-loading sections);
- `CSharpCompilerWasm/wwwroot/index.html` (title and stylesheet link);
- `.github/workflows/deploy.yml` (fork-specific base path and deployment steps).

When resolving conflicts, preserve the `PortableExecutableReference` fix and keep the deployment base href equal to the fork's final sub-path.

## 8. License

This fork is distributed under the MIT License. The original copyright notice remains:

```text
Copyright (c) 2025 Microsoft
```

See [LICENSE](LICENSE) for the full text.
