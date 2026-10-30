# iframe 样式 API

**简体中文** | [English](iframe-style-api.md)

游乐场会从页面查询字符串读取视觉参数，在普通浏览器标签页和 `<iframe>` 的 `src` 中都可以使用，无需重新构建或修改服务器配置。

## 快速开始

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?theme=light&accent=emerald&density=compact&chrome=minimal&radius=12"
  width="100%"
  height="560"
  frameborder="0"
  loading="lazy"
  title="C# Playground"></iframe>
```

参数名不区分大小写。无法识别的值会回退到默认值。

> 编译期目标参数 `target=net8.0|net9.0|net10.0` 请参阅 [目标框架选择](target-frameworks.zh-CN.md)。真实运行时选择（`/net8/`、`/net9/`、`/net10/` 或 `?runtime=...`）请参阅 [运行时版本](runtimes.zh-CN.md)。

游乐场顶部还提供实时主题和强调色选择器。它们只改变当前页面；iframe 初始样式仍建议通过 URL 参数指定。

## 参数说明

| 参数 | 类型 | 可选值 | 默认值 | 效果 |
| --- | --- | --- | --- | --- |
| `theme` | 枚举 | `dark`、`light`、`auto` | `dark` | 外层配色。`auto` 跟随系统 `prefers-color-scheme`。 |
| `accent` | 枚举 / 十六进制 | `indigo`、`blue`、`emerald`、`amber`、`rose`、`violet`、`cyan`、`orange`，或不带 `#` 的 6 位十六进制色值 | `indigo` | 按钮、焦点环、激活状态和强调光晕。 |
| `layout` | 枚举 | `split`、`stacked` | `split` | `split` 为编辑器/控制台左右分栏；`stacked` 为上下堆叠。 |
| `chrome` | 枚举 | `full`、`minimal`、`none` | `full` | `full` 保留应用页头；`minimal`/`none` 隐藏页头并压缩外围留白。 |
| `density` | 枚举 | `comfortable`、`compact` | `comfortable` | 减小内边距、间距和按钮尺寸。 |
| `radius` | 数字 | `0`–`32` | `16` | 卡片圆角，单位 px；超出范围会被限制。 |
| `fontsize` | 数字 | `11`–`22` | `14` | 编辑器字号，单位 px；超出范围会被限制。 |

### 强调色预设

| 名称 | Hex |
| --- | --- |
| `indigo` | `#6366f1` |
| `blue` | `#3b82f6` |
| `emerald` | `#10b981` |
| `amber` | `#f59e0b` |
| `rose` | `#f43f5e` |
| `violet` | `#8b5cf6` |
| `cyan` | `#06b6d4` |
| `orange` | `#f97316` |

自定义颜色请传 6 位十六进制值，不带 `#`：

```text
?accent=ff7a18
```

如果值既不是已知名称也不是合法的 6 位十六进制值，应用会回退到 `indigo`。

## 示例

### 浅色博客卡片

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?theme=light&accent=blue&chrome=minimal&density=compact"
  width="100%" height="520" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

### 深色终端风格

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?theme=dark&accent=emerald&density=comfortable&radius=8"
  width="100%" height="680" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

### 手机友好的上下布局

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?layout=stacked&chrome=none&fontsize=13"
  width="100%" height="760" frameborder="0" loading="lazy"
  title="C# Playground"></iframe>
```

## 根容器暴露的 CSS 变量

如需额外自定义 CSS，可使用以下变量：

```css
--accent: #6366f1;
--accent-rgb: 99, 102, 241;
--radius: 16px;
--editor-font-size: 14px;
```

主要样式文件为 `CSharpCompilerWasm/wwwroot/css/playground.css`。

## 托管与嵌入要求

- iframe URL 必须位于应用部署的 base 路径下，例如 `/csharp-roslyn-wasm-playground/`。
- 不要为游乐场响应添加 `X-Frame-Options: SAMEORIGIN` 或 `DENY`。
- 如使用 CSP，请通过 `frame-ancestors` 允许嵌入它的博客域名。
- 为 iframe 添加有意义的 `title` 属性以提升可访问性。
- `chrome=none` 会隐藏应用标题，但保留编辑器、控制台和运行按钮。

## 常见问题

| 现象 | 检查项 |
| --- | --- |
| 主题没有变化 | 确认查询参数在 iframe 的 URL 上，而不是父页面 URL；部署后尝试强制刷新。 |
| `_framework` 文件 404 | 检查 `.nojekyll`、SPA fallback 和部署的 base href。 |
| iframe 空白或被阻止 | 检查 `X-Frame-Options`、CSP `frame-ancestors` 和混合内容限制。 |
| HTML 中的 `&` | 如果模板要求转义，请写成 `&amp;`。 |
| 自定义强调色无效 | 使用不带 `#` 的 6 位十六进制值，或将 `#` 编码为 `%23`。 |
