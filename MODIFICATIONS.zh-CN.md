# 与上游项目的差异说明

**简体中文** | [English](MODIFICATIONS.md)

本文档说明本仓库相对于上游项目 [MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor) 做了哪些修改、修复和扩展。它不是完整更新日志；上游历史与版本仍以上游仓库为准。

**本分支使用的上游基线：** `MicrosoftLearning/c-sharp-minor` 的 `main` 分支，提交 `dad9656533d764c0272f3fa2a986ca00de28aaca`。

## 1. 变更概览

| 方面 | 上游 | 本分支 | 原因 |
| --- | --- | --- | --- |
| 构建产物 | Blazor 默认 Webcil 输出 | `<WasmEnableWebcil>false</WasmEnableWebcil>` | 在 `_framework` 中保留标准 `.dll`，方便浏览器端工具链和排查 |
| Roslyn 引用发现 | 使用 `PropertyType == typeof(MetadataReference)` 精确判断 | 改为 `typeof(MetadataReference).IsAssignableFrom(...)` | `Basic.Reference.Assemblies.Net90` 返回 `PortableExecutableReference`，上游写法会加载到 0 个引用 |
| 编译目标 | 固定 .NET 9 引用集 | 可选择 .NET 8 / .NET 9 / .NET 10 引用集 | 让用户按所选框架 API 面编译 |
| 运行时部署 | 单一 .NET 9 构建 | `/net8/`、`/net9/`、`/net10/` 三个真实运行时构建 | 让 iframe URL 和链接可以选择实际 Blazor 运行时 |
| 实时外观 | 只能通过 URL 参数 | 顶部主题和强调色选择器 | 无需刷新或重新构建即可切换外观 |
| 编辑器辅助 | 纯 textarea | 语义 C# 代码提示弹窗 | 输入时通过 `SemanticModel.LookupSymbols` 和成员查找提供提示 |
| 任务执行 | Roslyn 和用户代码都运行在 UI 线程 | Roslyn 编译、语义补全和用户代码执行均运行在独立 .NET Web Worker，并提供 Stop 按钮 | 补全和编译期间保持输入、滚动流畅，并隔离失控程序 |
| 运行反馈 | 静态按钮 | 带动画且保证最短显示时长的等待状态 | 更清晰地反馈编译/执行状态 |
| UI | 单栏教学布局 | 响应式双栏卡片工作台 | 提升可读性，更适合 iframe 嵌入 |
| 主题 | 固定的 Bootstrap/内联颜色 | URL 参数驱动的主题与预设 | 不同博客文章无需重新构建即可使用不同外观 |
| 部署 | 上游 GitHub Pages 路径 `/c-sharp-minor/` | 本分支路径 `/csharp-roslyn-wasm-playground/` 及辅助脚本 | 作为独立站点部署 |
| 文档 | 上游英文 README/安全策略 | 中英双语 README、安全策略和本差异说明 | 明确分支定位，方便中英文用户 |

## 2. 构建与 WebAssembly 运行时

### `CSharpCompilerWasm/CSharpCompilerWasm.csproj`

新增：

```xml
<WasmEnableWebcil>false</WasmEnableWebcil>
```

原因：本项目期望发布后的 `_framework` 目录保留标准 `.dll` 文件。关闭 Webcil 便于浏览器端工具检查产物，也避免把 Webcil 打包假设和 Roslyn/工具链流程混在一起。

项目文件的其他依赖与上游保持一致：.NET 9、Roslyn 4.14.0、`Basic.Reference.Assemblies.Net90`。

## 3. Roslyn 元数据引用修复

### `CSharpCompilerWasm/Pages/Compiler.razor`

上游通过反射查找引用程序集：

```csharp
.Where(p => p.PropertyType == typeof(MetadataReference))
```

但 `Basic.Reference.Assemblies.Net90.References` 暴露的属性类型是 `PortableExecutableReference`，它继承自 `MetadataReference`。精确类型相等会导致找不到任何引用，最终编译失败。

本分支改为：

```csharp
.Where(p => typeof(MetadataReference).IsAssignableFrom(p.PropertyType))
```

同时保留了“加载到多少个引用”的诊断输出，便于快速定位此类问题。

### 可选择的编译目标

本分支在 `Net90` 之外增加了 `Basic.Reference.Assemblies.Net80`。编辑器工具栏和 `target` URL 参数用于选择 Roslyn 使用的引用集（默认 `net9.0`，可选 `net8.0`）。

需要明确：这是**编译期引用切换**，不是运行时切换；Blazor 应用本身仍运行在 .NET 9 上。详见 [documentation/target-frameworks.zh-CN.md](documentation/target-frameworks.zh-CN.md)。

## 4. 界面重构与 iframe 主题

### 新增样式文件

`CSharpCompilerWasm/wwwroot/css/playground.css` 是本分支新增的完整视觉层，包含：

- 深色、浅色和 `auto` 主题；
- 双栏编辑器/控制台工作区；
- 窄屏上下堆叠布局；
- 紧凑密度模式；
- 可访问性焦点状态和减少动态效果支持；
- 适合 iframe 嵌入的展示样式。

### Razor 标记与代码

`CSharpCompilerWasm/Pages/Compiler.razor` 现在会：

- 在根容器上添加 `app-shell`、主题、布局、chrome 和密度类名；
- 从查询字符串读取 iframe/URL 样式参数；
- 输出 `--accent`、`--accent-rgb`、`--radius` 和 `--editor-font-size` CSS 变量；
- 将页面标题和页头文案改为 “C# Playground”；
- 为运行按钮增加可见的“运行代码”文字；
- 将控制台输入框改为使用主题 CSS 类，而不是硬编码内联颜色。

支持的 URL 参数：

| 参数 | 可选值 | 默认值 |
| --- | --- | --- |
| `theme` | `dark`、`light`、`auto` | `dark` |
| `accent` | `indigo`、`blue`、`emerald`、`amber`、`rose`、`violet`、`cyan`、`orange`，或不带 `#` 的 6 位十六进制色值 | `indigo` |
| `layout` | `split`、`stacked` | `split` |
| `chrome` | `full`、`minimal`、`none` | `full` |
| `density` | `comfortable`、`compact` | `comfortable` |
| `radius` | `0`–`32` | `16` |
| `fontsize` | `11`–`22` | `14` |

示例：

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/?theme=light&accent=emerald&density=compact&chrome=minimal"
  width="100%" height="560" frameborder="0" loading="lazy"></iframe>
```

### 宿主页面

`CSharpCompilerWasm/wwwroot/index.html` 现在会加载 `css/playground.css`，标题改为 `C# Playground`。

## 5. 部署与托管

本分支不再使用上游的 `/c-sharp-minor/` 路径，而是使用：

`/csharp-roslyn-wasm-playground/`

站点包含 `/net8/`、`/net9/`、`/net10/` 三个真实运行时构建，根路径默认使用 .NET 10 以兼容旧链接。详见 [documentation/runtimes.zh-CN.md](documentation/runtimes.zh-CN.md)。

相关改动包括：

- 本分支专用的 `.github/workflows/deploy.yml`，负责构建三个目标框架、重写各自 base href 并部署；
- `scripts/publish.sh` 和 `scripts/publish.ps1`，用于自托管发布；
- `deploy/nginx.conf.example`，包含 SPA fallback 和 WASM/DLL MIME 类型配置；
- GitHub Pages 的 `.nojekyll` 处理；
- iframe 兼容说明：不要为游乐场响应添加 `X-Frame-Options: SAMEORIGIN` 或 `DENY`。

构建和部署命令见 [README.zh-CN.md](README.zh-CN.md)。

## 6. 文档与开源归属

本分支新增：

- `README.md`（英文）和 `README.zh-CN.md`（中文），顶部可互相切换；
- `SECURITY.md`（英文）和 `SECURITY.zh-CN.md`（中文）；
- 本中英双语差异说明文档。

文档明确说明本项目是社区分支，并在 [LICENSE](LICENSE) 中保留上游 MIT 许可证和 Microsoft 版权声明。

## 7. 与上游同步

如需合入未来的上游改动：

```bash
git remote add upstream https://github.com/MicrosoftLearning/c-sharp-minor.git
git fetch upstream
git merge upstream/main
```

预计可能冲突的文件：

- `CSharpCompilerWasm/Pages/Compiler.razor`（UI 与引用加载部分）；
- `CSharpCompilerWasm/wwwroot/index.html`（标题和样式表链接）；
- `.github/workflows/deploy.yml`（本分支专用 base 路径和部署步骤）。

解决冲突时，请保留 `PortableExecutableReference` 修复，并确保部署 base href 与本分支最终子路径一致。

## 8. 许可证

本分支采用 MIT 许可证，原始版权声明保留如下：

```text
Copyright (c) 2025 Microsoft
```

完整文本见 [LICENSE](LICENSE)。
