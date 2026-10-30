# AI 与自动化集成

[English](ai-integration.md) | **简体中文**

本页是面向 AI 助手和博客自动化工具的稳定集成说明。

机器可读的摘要见 [`AGENTS.md`](../AGENTS.md) 和 [`llms.txt`](../llms.txt)。

## 工具是什么

这是一个静态 Blazor WebAssembly 应用。Roslyn 编译、语义补全和用户代码执行都在浏览器中完成。没有后端 API，没有服务端编译接口，也不需要认证。

## 固定地址

- 主站：`https://csharp-roslyn-wasm-playground.pages.dev/`
- GitHub Pages 镜像：`https://jokevallen.github.io/csharp-roslyn-wasm-playground/`
- .NET 8：`https://csharp-roslyn-wasm-playground.pages.dev/net8/`
- .NET 9：`https://csharp-roslyn-wasm-playground.pages.dev/net9/`
- .NET 10：`https://csharp-roslyn-wasm-playground.pages.dev/net10/`

根路径默认使用 .NET 10。

## 推荐 iframe

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

嵌入规则：

- 每个文章小节放一个即可，不要在单页堆很多个。
- `theme` 和 `accent` 与博客页面匹配。
- 博客嵌入建议 `chrome=minimal`。
- 使用 `completion=manual`，避免页面一打开就启动 Worker。
- 保留 `loading="lazy"` 和有意义的 `title`。
- 博客平台必须允许原始 HTML/iframe。

完整参数见 [iframe 样式 API](iframe-style-api.zh-CN.md)。

## 给 AI 的提示词模板

```text
在文章中嵌入 C# Playground 时：

- 地址：https://csharp-roslyn-wasm-playground.pages.dev/
- 默认参数：?theme=light&accent=indigo&layout=stacked&chrome=minimal&completion=manual
- 使用懒加载 iframe，width="100%"，height="720"，并添加 title。
- 深色文章使用 theme=dark。
- 不要声称存在编译/运行的 HTTP API。
- 不要在示例中放入密钥或隐私数据。
```

## 浏览器自动化

没有 API，需要 Playwright 等浏览器自动化工具：

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

选择器：

| 元素 | 选择器 |
| --- | --- |
| 代码编辑器 | `#code-editor` |
| 运行按钮 | `.compile-btn` |
| 输出区域 | `#output` |

不要使用 `networkidle`；.NET 运行时会下载大量资源。应使用 `domcontentloaded` 并等待 `#code-editor`。

## 当前限制

- 没有 `?code=` 预填参数。
- 没有 `postMessage` 运行/结果协议。
- 没有服务端无头编译/运行 API。
- `Console.ReadLine()` 需要读者在 UI 中输入。
- 首次冷加载会下载较大的运行时，可能需要 10–30 秒。

## 相关文档

- [项目 README](../README.zh-CN.md)
- [浏览器运行时](browser-runtime.zh-CN.md)
- [Console API](console-api.zh-CN.md)
- [部署](deployment.zh-CN.md)
- [与上游差异](../MODIFICATIONS.zh-CN.md)
