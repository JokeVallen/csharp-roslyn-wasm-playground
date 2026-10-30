# C# Playground · Blazor WASM + Roslyn

[简体中文](README.zh-CN.md) | **English**

A browser-based C# playground for writing, compiling, and running C# without a backend. It is built with Blazor WebAssembly, the Roslyn compiler, and .NET WebAssembly, and is designed to be embedded in blogs or documentation through an `<iframe>`.

This repository is an extended fork of [MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor). The original project was created for the [Get started with C# Programming](https://learn.microsoft.com/training/paths/get-started-c-sharp-part-1) learning path. This fork keeps the browser-only Roslyn compilation model, supports real .NET 8 / .NET 9 / .NET 10 runtime builds, adds a themeable UI, iframe presets, code completion, and GitHub Pages / Cloudflare Pages deployment pipelines.

> **NOTE**: This is a community fork. It is not an officially supported Microsoft product or service and is provided as-is, without warranty of any kind. The original MIT license and copyright notice are retained in [LICENSE](LICENSE).

**Live demo:** [https://csharp-roslyn-wasm-playground.pages.dev/](https://csharp-roslyn-wasm-playground.pages.dev/)  
**Original project:** [https://github.com/MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor)

## ✨ Features

- **Real C# compilation in the browser**: Roslyn compiles user code directly in WebAssembly; no server-side execution.
- **Interactive console**: Supports `Console.Write`, `Console.WriteLine`, and `Console.ReadLine()` through JS interop.
- **Learning-friendly editor**: Toggle between simplified method-body mode and full class view, with automatic/manual semantic completion, conditional Tab completion/indentation, and Roslyn-based formatting.
- **Live appearance controls**: Switch dark/light/system theme and accent color directly in the toolbar at runtime.
- **Run feedback**: Animated waiting state on the Run button while compilation/execution is in progress.
- **Sample code and diagnostics**: Built-in sample, real Roslyn diagnostics, optional system output.
- **Themeable iframe presets**: Change theme, accent, layout, density, radius, and font size from URL parameters.
- **Web Worker execution sandbox**: Roslyn compilation, semantic completion, and user-code execution run off the UI thread; a Stop button terminates runaway programs.
- **Compact editor settings**: Compile target, automatic completion, and format-after-completion live in a `⚙ 设置` popover.
- **Bilingual docs**: English and 简体中文 documentation for the runtime, iframe API, deployment, and fork changes.
- **Static deployment**: Publishes as a self-contained static site for GitHub Pages, Cloudflare Pages, Nginx, or any SPA-capable host.
- **Open source attribution**: Extended from `MicrosoftLearning/c-sharp-minor`; MIT licensed.







## 🏗️ Architecture

### Technology Stack
- **Framework**: Blazor WebAssembly, multi-targeted to .NET 8, .NET 9, and .NET 10
- **UI**: Razor components + Bootstrap base styles + custom themeable CSS (`wwwroot/css/playground.css`)
- **Compiler**: Microsoft.CodeAnalysis.CSharp (Roslyn) 4.14.0, running in a dedicated .NET Web Worker so editing stays responsive
- **Editor**: Native `<textarea>` with keyboard shortcuts and simplified/full-code modes (no Monaco dependency)
- **Reference assemblies**: Basic.Reference.Assemblies.Net80 / Net90 / Net100 embedded resources for WebAssembly-compatible Roslyn references
- **Deployment**: GitHub Pages and Cloudflare Pages workflows + reusable `scripts/publish.*` helpers

### Key implementation notes

- `<WasmEnableWebcil>false</WasmEnableWebcil>` is required so the published `_framework` output keeps `.dll` files available for browser-side tooling.
- Roslyn references are loaded from the selected `Basic.Reference.Assemblies.Net80` / `Net90` / `Net100` reference set. The properties return `PortableExecutableReference`, so reference discovery must use `typeof(MetadataReference).IsAssignableFrom(...)`, not exact type equality.
- Three real runtime builds are published under `/net8/`, `/net9/`, and `/net10/`; the root serves the .NET 10 build by default. See [Runtime versions](documentation/runtimes.md).
- The final deployment path must match `<base href="...">` and include a trailing slash.
- GitHub Pages needs a `.nojekyll` file so `_framework` is not ignored.

## 🛠️ Getting started

Requirements: .NET 8, .NET 9, and/or .NET 10 SDK, depending on which runtime build you want to run locally.

```bash
dotnet restore
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net10.0
```

Open the URL printed by `dotnet` (the HTTP profile defaults to `http://localhost:5158`).

Production publish:

```bash
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net10.0 -o publish
```

## 📁 Project structure

```text
CSharpCompilerWasm/
  Pages/Compiler.razor        # playground UI, worker requests, execution, iframe options
  RoslynWorker.cs             # [JSExport] Roslyn compile/completion methods for the worker
  ConsoleReplacementCode.cs   # in-browser Console replacement injected into user code
  wwwroot/worker.js           # .NET Web Worker entry point
  wwwroot/js/roslyn-worker.js # main-thread worker client
  wwwroot/css/playground.css  # themeable UI and iframe presets
  wwwroot/index.html          # Blazor host page / base href
.github/workflows/             # GitHub Pages and Cloudflare Pages deployment
scripts/publish.*             # self-hosted publish helpers
deploy/nginx.conf.example     # example Nginx SPA configuration
```

## 🚀 Deployment

The primary deployment is Cloudflare Pages and updates automatically on every push to `main`:

**https://csharp-roslyn-wasm-playground.pages.dev/**

GitHub Pages remains available as a mirror:

**https://jokevallen.github.io/csharp-roslyn-wasm-playground/**

The GitHub Pages workflow in `.github/workflows/deploy.yml`:

1. runs `dotnet publish -c Release -f net8.0|net9.0|net10.0` for each runtime,
2. rewrites each build's `<base href="...">` and assembles the multi-runtime site,
3. adds `.nojekyll`, and
4. publishes the assembled `publish/site` through GitHub Pages.

The workflow in `.github/workflows/deploy-cloudflare.yml` builds the same multi-runtime site for Cloudflare Pages, strips precompressed files that Cloudflare regenerates, writes `_headers`, and deploys through Wrangler. Configure `CLOUDFLARE_API_TOKEN` and `CLOUDFLARE_ACCOUNT_ID` as repository secrets; optionally set `CLOUDFLARE_PROJECT_NAME` as a repository variable.

For a self-hosted/dsh deployment under a different path, use the helper scripts:

```bash
./scripts/publish-all.sh /csharp-playground/ ./publish/site
# or on Windows:
./scripts/publish-all.ps1 -RootBaseHref /csharp-playground/ -OutputDir ./publish/site
```

Then serve `publish/site` with an SPA fallback. See `deploy/nginx.conf.example`.
Remember that the `<base href>` must match the final URL path and must end with `/`.

## 🎨 iframe style parameters

The playground can be themed from the iframe `src` URL, so different blog posts
can use different looks without rebuilding the app:

| Parameter | Values | Default |
| --- | --- | --- |
| `theme` | `dark`, `light`, `auto` | `dark` |
| `accent` | `indigo`, `blue`, `emerald`, `amber`, `rose`, `violet`, `cyan`, `orange`, or a 6-digit hex color without `#` | `indigo` |
| `layout` | `split`, `stacked` | `split` |
| `chrome` | `full`, `minimal`, `none` | `full` |
| `density` | `comfortable`, `compact` | `comfortable` |
| `radius` | `0`–`32` | `16` |
| `fontsize` | `11`–`22` | `14` |
| `target` | `net10.0`, `net9.0`, `net8.0` (depends on runtime build) | runtime default |
| `completion` | `auto`, `manual` | `auto` |

`target` selects the compile-time reference assemblies. `completion=auto` requests semantic completion after a short idle and keeps the local keyword popup during typing. The active runtime is chosen by the URL path; see [Runtime versions](documentation/runtimes.md).

Examples:

```html
<!-- Light, emerald, compact blog card -->
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=emerald&density=compact&chrome=minimal&radius=12"
  width="100%" height="560" frameborder="0" loading="lazy"></iframe>

<!-- Stacked phone-friendly layout with custom accent -->
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=dark&accent=ff7a18&layout=stacked&fontsize=13"
  width="100%" height="760" frameborder="0" loading="lazy"></iframe>
```


## 🔐 Security

This project is an educational playground, not a production sandbox. User code runs in the browser with the permissions of the page. Do not use it to run untrusted code in a sensitive context. See [SECURITY.md](SECURITY.md) for the reporting process.

## 🤝 Contributing

Issues and pull requests are welcome. Please keep changes focused, include a short description, and verify the playground can compile and run a simple program before submitting. For larger changes, open an issue first.

## 📜 Attribution and license

- Original project: [MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor)
- This fork: [JokeVallen/csharp-roslyn-wasm-playground](https://github.com/JokeVallen/csharp-roslyn-wasm-playground)
- License: MIT. The original copyright notice is retained in [LICENSE](LICENSE).

The name "C# Playground" and this fork's UI are community additions and are not official Microsoft branding.


## 📚 Documentation

- [Documentation index](documentation/README.md)
- [Differences from the upstream project](MODIFICATIONS.md)
- [与上游项目的差异说明](MODIFICATIONS.zh-CN.md)
- [Security policy](SECURITY.md)
