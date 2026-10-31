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
| `code` | base64url-encoded C# source | none |
| `readonly` | `1` / `true` | off |

## Prefill code from a URL

```text
https://csharp-roslyn-wasm-playground.pages.dev/?code=BASE64URL&theme=light&chrome=minimal
```

Encode UTF-8 source as base64url (replace `+` with `-`, `/` with `_`, remove `=` padding). Add `readonly=1` for a non-editable example.

## postMessage API

The iframe exposes a browser-only API; there is still no HTTP compile endpoint.

Parent page to iframe:

```javascript
iframe.contentWindow.postMessage({
  type: 'csharp-playground:run',
  requestId: 'req-1',
  code: 'Console.WriteLine("hello");'
}, 'https://csharp-roslyn-wasm-playground.pages.dev');
```

Iframe response:

```javascript
{
  type: 'csharp-playground:result',
  requestId: 'req-1',
  output: 'hello\n'
}
```

Errors return `{ type, requestId, error }`. This API is intended for trusted embedders. `Console.ReadLine()` is not supported by the postMessage runner.

## Editor features

- Local snippet completion for `for`, `foreach`, `if`, `while`, `do`, `switch`, and `try`, with a collapsible and resizable preview pane; inserted multiline snippets inherit the current line indentation.
- `Tab` accepts a suggestion when the menu is open; otherwise it indents. `Enter` auto-indents and adds one level after `{`, `(`, or `[`; typing `}` dedents; multiline paste is re-indented relative to the current line.
- `Ctrl + Shift + F` formats code.
- Settings can configure local/semantic/format delays, format triggers, shortcut recording, and idle worker prewarm.
- Click outside the settings popover or press `Escape` to close it.

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
- `postMessage` still requires a real browser page and Web Worker.
- `Console.ReadLine()` is interactive and not supported by the postMessage runner.
- URL-prefilled code is limited by practical URL length; keep examples short.
- User code runs entirely in the browser in a Web Worker sandbox.
- Do not put secrets or private data into generated examples.

## Documentation

- [AI integration guide](documentation/ai-integration.md)
- [Iframe style API](documentation/iframe-style-api.md)
- [Browser runtime](documentation/browser-runtime.md)
- [Console API](documentation/console-api.md)
- [Runtime versions](documentation/runtimes.md)
- [Deployment guide](documentation/deployment.md)
