# Documentation

[简体中文](README.zh-CN.md) | **English**

This directory contains focused documentation for the playground's user-facing features, APIs, and deployment workflow. The generated static site in `docs/` is a publish artifact; the Markdown files here are the documentation source.

## Documents

| Document | What it covers |
| --- | --- |
| [AI and agent integration](ai-integration.md) | Stable URLs, iframe rules, Playwright selectors, and limitations for AI/automation |
| [Iframe style API](iframe-style-api.md) | URL parameters for theme, accent, layout, chrome, density, radius, and font size |
| [Browser C# runtime](browser-runtime.md) | How Roslyn compilation and WebAssembly execution work, supported code shape, and limitations |
| [Console API reference](console-api.md) | The `Console` subset injected into user code |
| [Runtime versions](runtimes.md) | Real .NET 8 / .NET 9 / .NET 10 deployments and URL layout |
| [Target framework selection](target-frameworks.md) | Compile user code against .NET 8 / .NET 9 / .NET 10 reference assemblies |
| [Static deployment guide](deployment.md) | Cloudflare Pages, GitHub Pages, self-hosted publishing, base href, SPA fallback, and iframe headers |
| [Differences from upstream](../MODIFICATIONS.md) | What this fork changes compared with `MicrosoftLearning/c-sharp-minor` |
| [Project README](../README.md) | Features, quick start, and repository overview |

## Language policy

Each document has a `*.zh-CN.md` counterpart and a language switch at the top. Keep both versions in sync when changing behavior or public parameters.

## Related

- [Security policy](../SECURITY.md)
- [License](../LICENSE)
