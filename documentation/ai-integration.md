# AI and agent integration

[简体中文](ai-integration.zh-CN.md) | **English**

This page is the stable integration guide for AI assistants and automated blog tooling.

For a machine-oriented summary, see [`AGENTS.md`](../AGENTS.md) and [`llms.txt`](../llms.txt).

## What the playground is

The playground is a static Blazor WebAssembly application. Roslyn compilation, semantic completion, and user-code execution run in the browser. There is no backend API, no server-side compile endpoint, and no authentication.

## Canonical URLs

- Primary: `https://csharp-roslyn-wasm-playground.pages.dev/`
- GitHub Pages mirror: `https://jokevallen.github.io/csharp-roslyn-wasm-playground/`
- .NET 8: `https://csharp-roslyn-wasm-playground.pages.dev/net8/`
- .NET 9: `https://csharp-roslyn-wasm-playground.pages.dev/net9/`
- .NET 10: `https://csharp-roslyn-wasm-playground.pages.dev/net10/`

The root URL defaults to .NET 10.

## Recommended iframe

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

- One embed per article section is usually enough.
- Match `theme` and `accent` to the host page.
- Use `chrome=minimal` for blog embeds.
- Use `completion=manual` to avoid starting the worker automatically.
- Keep `loading="lazy"` and a descriptive `title`.
- The blog platform must allow raw HTML/iframe blocks.

See [Iframe style API](iframe-style-api.md) for all URL parameters.

## Prefill code with `?code=`

Encode UTF-8 source as base64url, then pass it as `code`:

```text
https://csharp-roslyn-wasm-playground.pages.dev/?code=BASE64URL&theme=light&chrome=minimal
```

Add `readonly=1` for a non-editable example. This is suitable for short examples; keep URLs within practical browser limits.

## Run from a parent page with `postMessage`

The iframe exposes a browser-only automation API:

```javascript
iframe.contentWindow.postMessage({
  type: 'csharp-playground:run',
  requestId: 'req-1',
  code: 'Console.WriteLine("hello");'
}, 'https://csharp-roslyn-wasm-playground.pages.dev');
```

Response:

```javascript
{
  type: 'csharp-playground:result',
  requestId: 'req-1',
  output: 'hello\n'
}
```

Failures return `{ type, requestId, error }`. Only use this with trusted embedders. `Console.ReadLine()` is not supported by this runner.

## Agent prompt template

```text
When embedding the C# Playground:

- URL: https://csharp-roslyn-wasm-playground.pages.dev/
- Default query: ?theme=light&accent=indigo&layout=stacked&chrome=minimal&completion=manual
- Use a lazy-loaded iframe with width="100%", height="720", and a descriptive title.
- Use theme=dark when the article uses a dark theme.
- Do not claim there is a compile/run HTTP API.
- Do not place secrets or private data in examples.
```

## Browser automation

There is no API. Use Playwright or a similar browser automation tool:

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

Selectors:

| Element | Selector |
| --- | --- |
| Code editor | `#code-editor` |
| Run button | `.compile-btn` |
| Output | `#output` |

Do not use `networkidle`; the .NET runtime downloads many assets. Use `domcontentloaded` and wait for `#code-editor`.

## Local smoke test

The repository includes `scripts/smoke-test.js`:

```bash
npm install playwright
node scripts/smoke-test.js http://127.0.0.1:5158/
```

It checks page load, compilation, `postMessage` execution, and snippet completion.

## Current limitations

- No headless HTTP compile/run API.
- `postMessage` still requires a real browser page and Web Worker.
- `Console.ReadLine()` requires interactive UI input and is not supported by the `postMessage` runner.
- URL-prefilled code is limited by practical URL length; keep examples short.
- First load downloads a large runtime and can take 10–30 seconds on a cold cache.

## Related

- [Project README](../README.md)
- [Browser runtime](browser-runtime.md)
- [Console API](console-api.md)
- [Deployment](deployment.md)
- [Differences from upstream](../MODIFICATIONS.md)
