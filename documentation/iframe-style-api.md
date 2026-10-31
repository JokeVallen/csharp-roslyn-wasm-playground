# Iframe style API

[简体中文](iframe-style-api.zh-CN.md) | **English**

The playground reads visual options from the page query string. They work in a normal browser tab and in an `<iframe>` `src`; no rebuild or server-side configuration is required.

## Quick start

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=emerald&density=compact&chrome=minimal&radius=12"
  width="100%"
  height="560"
  frameborder="0"
  loading="lazy"
  title="C# Playground"></iframe>
```

Parameter names are case-insensitive. Unknown values fall back to the documented defaults.

> For the compile-time `target=net8.0|net9.0|net10.0` option, see [Target framework selection](target-frameworks.md). For real runtime selection (`/net8/`, `/net9/`, `/net10/` or `?runtime=...`), see [Runtime versions](runtimes.md).

The playground header also provides live theme and accent selectors. They change the current page only; URL parameters remain the recommended way to set the initial style for an iframe.

The `⚙ 设置` popover repositions itself inside the iframe viewport and scrolls internally, so compact embeds do not clip its lower controls.

## Parameters

| Parameter | Type | Allowed values | Default | Effect |
| --- | --- | --- | --- | --- |
| `theme` | enum | `dark`, `light`, `auto` | `dark` | Outer shell color scheme. `auto` follows `prefers-color-scheme`. |
| `accent` | enum / hex | `indigo`, `blue`, `emerald`, `amber`, `rose`, `violet`, `cyan`, `orange`, or 6-digit hex without `#` | `indigo` | Buttons, focus rings, active controls, and accent glow. |
| `layout` | enum | `split`, `stacked` | `split` | `split` = editor and console side by side; `stacked` = editor above console. |
| `chrome` | enum | `full`, `minimal`, `none` | `full` | `full` keeps the app header; `minimal`/`none` hide it and compact the surrounding chrome. |
| `density` | enum | `comfortable`, `compact` | `comfortable` | Reduces padding, gaps, and button sizes. |
| `radius` | number | `0`–`32` | `16` | Card corner radius in CSS pixels. Values are clamped. |
| `fontsize` | number | `11`–`22` | `14` | Editor font size in CSS pixels. Values are clamped. |
| `completion` | enum | `auto`, `manual` | `auto` | Automatic semantic completion or manual trigger only. |
| `target` | enum | `net8.0`, `net9.0`, `net10.0` | runtime default | Compile against the selected reference assemblies. |
| `code` | string | base64url-encoded C# source | none | Prefills the editor. Keep examples short. |
| `readonly` | bool | `1`, `true` | off | Makes the editor read-only. |

### Accent palette

| Name | Hex |
| --- | --- |
| `indigo` | `#6366f1` |
| `blue` | `#3b82f6` |
| `emerald` | `#10b981` |
| `amber` | `#f59e0b` |
| `rose` | `#f43f5e` |
| `violet` | `#8b5cf6` |
| `cyan` | `#06b6d4` |
| `orange` | `#f97316` |

For a custom color, pass six hex digits without `#`:

```text
?accent=ff7a18
```

If the value is not a known name and not six valid hex digits, the app uses `indigo`.

## Examples

### Light blog card

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=blue&chrome=minimal&density=compact"
  width="100%" height="520" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

### Dark terminal-style panel

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=dark&accent=emerald&density=comfortable&radius=8"
  width="100%" height="680" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

### Mobile-friendly stacked layout

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?layout=stacked&chrome=none&fontsize=13"
  width="100%" height="760" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

## CSS variables exposed by the shell

The root container exposes these variables for custom CSS if needed:

```css
--accent: #6366f1;
--accent-rgb: 99, 102, 241;
--radius: 16px;
--editor-font-size: 14px;
```

The main stylesheet is `CSharpCompilerWasm/wwwroot/css/playground.css`.

## Hosting and embedding requirements

- The iframe URL must be under the same base path as the deployed app, for example `/csharp-roslyn-wasm-playground/`.
- Do **not** send `X-Frame-Options: SAMEORIGIN` or `DENY` for the playground response.
- If using CSP, allow the embedding blog with `frame-ancestors`.
- Use a meaningful `title` attribute on the iframe for accessibility.
- `chrome=none` hides the app title but keeps the editor, console, and run button.

## Troubleshooting

| Symptom | Check |
| --- | --- |
| Theme does not change | Confirm the query string is on the iframe URL, not the parent page; hard-refresh after deployment. |
| 404 for `_framework` files | Check `.nojekyll`, SPA fallback, and the deployed base href. |
| iframe is blank/blocked | Check `X-Frame-Options`, CSP `frame-ancestors`, and mixed-content rules. |
| URL has `&` in HTML | In HTML source, write `&amp;` if your template requires escaped ampersands. |
| Custom accent ignored | Use six hex digits without `#`, or URL-encode `#` as `%23`. |
