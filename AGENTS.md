# AGENTS.md

This repository publishes a browser-only C# playground built with Blazor WebAssembly and Roslyn.

## Canonical URLs

- Primary live site: `https://csharp-roslyn-wasm-playground.pages.dev/`
- GitHub Pages mirror: `https://jokevallen.github.io/csharp-roslyn-wasm-playground/`
- .NET 8 runtime: `https://csharp-roslyn-wasm-playground.pages.dev/net8/`
- .NET 9 runtime: `https://csharp-roslyn-wasm-playground.pages.dev/net9/`
- .NET 10 runtime: `https://csharp-roslyn-wasm-playground.pages.dev/net10/`

The root URL defaults to the .NET 10 runtime.

## Recommended blog embedding

Use a lazy-loaded iframe with manual completion:

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=indigo&layout=stacked&chrome=minimal&completion=manual"
  width="100%"
  height="720"
  style="border:0;border-radius:12px;overflow:hidden;"
  loading="lazy"
  allow="clipboard-write"
  title="C# Playground">
</iframe>
```

Rules:

- Keep one playground per article section; do not embed many on one page.
- Use `theme=light|dark|auto` to match the host page.
- Use `layout=stacked` for narrow blog columns.
- Use `chrome=minimal` for embeds.
- Use `completion=manual` to avoid starting the Web Worker automatically.
- Always include `loading="lazy"`, a `title`, and `width="100%"`.
- Raw HTML/iframe blocks must be allowed by the blog platform.

Supported URL parameters:

| Parameter | Values | Default |
| --- | --- | --- |
| `theme` | `dark`, `light`, `auto` | `dark` |
| `accent` | `indigo`, `blue`, `emerald`, `amber`, `rose`, `violet`, `cyan`, `orange` | `indigo` |
| `layout` | `split`, `stacked` | `split` |
| `chrome` | `full`, `minimal`, `none` | `full` |
| `density` | `comfortable`, `compact` | `comfortable` |
| `radius` | `0`–`32` | `16` |
| `fontsize` | `11`–`22` | `14` |
| `completion` | `auto`, `manual` | `auto` |
| `target` | `net8.0`, `net9.0`, `net10.0` | build default |

## Browser automation

There is no HTTP API and no server-side execution endpoint. To validate code, drive the page with Playwright:

```javascript
const { chromium } = require('playwright');

const browser = await chromium.launch({ channel: 'msedge', headless: true });
const page = await browser.newPage();

await page.goto('https://csharp-roslyn-wasm-playground.pages.dev/', {
  waitUntil: 'domcontentloaded',
  timeout: 120000
});

await page.locator('#code-editor').waitFor({ timeout: 120000 });
await page.locator('#code-editor').fill('Console.WriteLine("Hello from agent");');
await page.locator('.compile-btn').click();

await page.waitForFunction(() => {
  return document.querySelector('#output')?.textContent?.includes('Hello from agent');
}, { timeout: 60000 });

await browser.close();
```

Important:

- Use `domcontentloaded`, not `networkidle`.
- Wait for `#code-editor`; first load downloads a large .NET runtime.
- Editor selector: `#code-editor`
- Run button selector: `.compile-btn`
- Output selector: `#output`
- First worker start can take 10–30 seconds.
- `Console.ReadLine()` is interactive in the UI and is not suitable for unattended automation.

## Limitations

- No headless HTTP compile/run API.
- No `?code=` prefill parameter yet.
- No `postMessage` run/result API yet.
- User code runs entirely in the browser in a Web Worker sandbox.
- Do not put secrets or private data into generated examples.

## Documentation

- [AI integration guide](documentation/ai-integration.md)
- [Iframe style API](documentation/iframe-style-api.md)
- [Browser runtime](documentation/browser-runtime.md)
- [Console API](documentation/console-api.md)
- [Runtime versions](documentation/runtimes.md)
- [Deployment guide](documentation/deployment.md)
