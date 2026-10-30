# C# Playground · Blazor WASM + Roslyn

**简体中文** | [English](README.md)

一个完全运行在浏览器中的 C# 在线游乐场：编写代码、调用 Roslyn 编译，并在 .NET WebAssembly 中直接执行，无需后端服务器。项目支持通过 `<iframe>` 嵌入博客或文档，并可通过 URL 参数切换多套视觉风格。

本仓库是 [MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor) 的扩展分支。原项目用于支持 Microsoft Learn 的 [Get started with C# Programming](https://learn.microsoft.com/training/paths/get-started-c-sharp-part-1) 学习路径；本分支在保留“浏览器内编译”架构的基础上，支持真实的 .NET 8 / .NET 9 / .NET 10 运行时构建，并加入可主题化界面、iframe 预设、代码提示和 GitHub Pages 部署流程。

> **说明**：本项目是社区分支，并非 Microsoft 官方支持的产品或服务。项目按“原样”提供，不附带任何形式的担保。原始 MIT 许可证及版权声明保留在 [LICENSE](LICENSE) 中。

**在线演示：** [https://jokevallen.github.io/csharp-roslyn-wasm-playground/](https://jokevallen.github.io/csharp-roslyn-wasm-playground/)  
**原项目：** [https://github.com/MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor)

## ✨ 功能特性

- **浏览器内真实编译**：使用 Roslyn 在 WebAssembly 中编译用户代码，不依赖服务器执行。
- **交互式控制台**：支持 `Console.Write`、`Console.WriteLine` 和 `Console.ReadLine()`，通过 JS Interop 处理输入输出。
- **适合学习的编辑器**：可在“简化方法体”和“完整类结构”两种代码视图间切换，并提供基于 `SemanticModel.LookupSymbols` 和成员查找的语义代码提示。
- **实时外观切换**：可直接在工具栏切换深色/浅色/跟随系统主题和强调色。
- **运行反馈**：编译执行期间运行按钮显示等待动画。
- **示例代码与诊断**：内置示例代码、真实 Roslyn 编译错误，以及可选的系统输出。
- **可主题化的 iframe 预设**：通过 URL 参数调整主题、强调色、布局、密度、圆角和字号。
- **Web Worker 执行沙盒**：Roslyn 编译、语义补全和用户代码执行均运行在 UI 线程之外；Stop 按钮可终止失控程序。
- **纯静态部署**：构建产物可部署到 GitHub Pages、Nginx 或任何支持 SPA fallback 的静态托管环境。
- **开源归属清晰**：基于 `MicrosoftLearning/c-sharp-minor` 扩展，采用 MIT 许可证。

## 🏗️ 技术架构

### 技术栈

- **框架**：Blazor WebAssembly，多目标 .NET 8 / .NET 9 / .NET 10
- **界面**：Razor 组件 + Bootstrap 基础样式 + 自定义主题样式（`wwwroot/css/playground.css`）
- **编译器**：Microsoft.CodeAnalysis.CSharp (Roslyn) 4.14.0，运行在独立的 .NET Web Worker 中，编辑时不再阻塞主线程
- **编辑器**：原生 `<textarea>`，支持快捷键及简化/完整代码视图（不依赖 Monaco）
- **引用程序集**：Basic.Reference.Assemblies.Net80 / Net90 / Net100 的内嵌资源，提供 WebAssembly 兼容的 Roslyn 引用
- **部署**：GitHub Pages 工作流 + 可复用的 `scripts/publish.*` 脚本

### 关键实现说明

- 必须在 `.csproj` 中设置 `<WasmEnableWebcil>false</WasmEnableWebcil>`，确保发布后的 `_framework` 保留可被浏览器端工具使用的 `.dll` 文件。
- Roslyn 引用来自所选的 `Basic.Reference.Assemblies.Net80` / `Net90` / `Net100` 引用集。这些属性返回 `PortableExecutableReference`，因此引用发现必须使用 `typeof(MetadataReference).IsAssignableFrom(...)`，不能使用精确类型相等判断。
- 发布 `.NET 8`、`.NET 9`、`.NET 10` 三个真实运行时构建，分别位于 `/net8/`、`/net9/`、`/net10/`；根路径默认使用 .NET 10。详见 [运行时版本](documentation/runtimes.zh-CN.md)。
- 最终部署路径必须与 `<base href="...">` 一致，且末尾斜杠不能省略。
- GitHub Pages 需要 `.nojekyll` 文件，避免 `_framework` 目录被忽略。

## 🛠️ 本地运行

环境要求：根据需要安装 .NET 8、.NET 9 和/或 .NET 10 SDK。

```bash
dotnet restore
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net10.0
```

打开 `dotnet` 输出的地址（HTTP 配置默认为 `http://localhost:5158`）。

生产构建：

```bash
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net10.0 -o publish
```

## 📁 项目结构

```text
CSharpCompilerWasm/
  Pages/Compiler.razor        # 游乐场 UI、Worker 请求、执行、iframe 参数
  RoslynWorker.cs             # 提供给 Worker 的 [JSExport] Roslyn 编译/补全方法
  ConsoleReplacementCode.cs   # 注入用户代码的浏览器端 Console 替代实现
  wwwroot/worker.js           # .NET Web Worker 入口
  wwwroot/js/roslyn-worker.js # 主线程 Worker 客户端
  wwwroot/css/playground.css  # 可主题化 UI 与 iframe 预设
  wwwroot/index.html          # Blazor 宿主页 / base href
.github/workflows/deploy.yml  # GitHub Pages 部署
scripts/publish.*             # 自托管发布脚本
deploy/nginx.conf.example     # Nginx SPA 配置示例
```

## 🚀 部署

每次推送到 `main` 分支都会自动部署到 GitHub Pages：

**https://jokevallen.github.io/csharp-roslyn-wasm-playground/**

`.github/workflows/deploy.yml` 会依次执行：

1. 分别执行 `dotnet publish -c Release -f net8.0|net9.0|net10.0`；
2. 重写每个构建的 `<base href="...">` 并组装多运行时站点；
3. 生成 `.nojekyll`；
4. 将组装后的 `publish/site` 发布到 GitHub Pages。

如果部署到 dsh 或其他自托管环境的子路径，可使用辅助脚本：

```bash
./scripts/publish-all.sh /csharp-playground/ ./publish/site
# 或 Windows：
./scripts/publish-all.ps1 -RootBaseHref /csharp-playground/ -OutputDir ./publish/site
```

随后使用支持 SPA fallback 的 Web 服务器托管 `publish/site`。可参考 `deploy/nginx.conf.example`。请务必保证 `<base href>` 与最终 URL 路径一致，并以 `/` 结尾。

## 🎨 iframe 样式参数

可以直接在 iframe 的 `src` 查询参数中切换外观，无需重新构建应用：

| 参数 | 可选值 | 默认值 |
| --- | --- | --- |
| `theme` | `dark`、`light`、`auto` | `dark` |
| `accent` | `indigo`、`blue`、`emerald`、`amber`、`rose`、`violet`、`cyan`、`orange`，或不带 `#` 的 6 位十六进制色值 | `indigo` |
| `layout` | `split`、`stacked` | `split` |
| `chrome` | `full`、`minimal`、`none` | `full` |
| `density` | `comfortable`、`compact` | `comfortable` |
| `radius` | `0`–`32` | `16` |
| `fontsize` | `11`–`22` | `14` |
| `target` | `net10.0`、`net9.0`、`net8.0`（取决于运行时构建） | 当前运行时默认值 |
| `completion` | `auto`、`manual` | `auto` |

`target` 选择编译期引用程序集。`completion=auto` 会在短暂停止输入后自动请求语义补全，输入过程中保留本地关键字提示。实际运行时由 URL 路径决定，详见 [运行时版本](documentation/runtimes.zh-CN.md)。

示例：

```html
<!-- 浅色、翠绿色强调、紧凑博客卡片 -->
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?theme=light&accent=emerald&density=compact&chrome=minimal&radius=12"
  width="100%" height="560" frameborder="0" loading="lazy"></iframe>

<!-- 深色、自定义橙色强调、上下布局 -->
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?theme=dark&accent=ff7a18&layout=stacked&fontsize=13"
  width="100%" height="760" frameborder="0" loading="lazy"></iframe>
```

## 🔐 安全说明

本项目是教学用在线游乐场，不是生产级沙箱。用户代码以页面的浏览器权限运行，请勿在敏感场景中用它执行不可信代码。漏洞报告流程见 [SECURITY.md](SECURITY.md)。

## 🤝 参与贡献

欢迎提交 Issue 和 Pull Request。提交前请保持改动聚焦、附上简短说明，并验证游乐场能够编译和运行简单程序。较大的改动建议先开 Issue 讨论。

## 📜 开源归属与许可证

- 原项目：[MicrosoftLearning/c-sharp-minor](https://github.com/MicrosoftLearning/c-sharp-minor)
- 本分支：[JokeVallen/csharp-roslyn-wasm-playground](https://github.com/JokeVallen/csharp-roslyn-wasm-playground)
- 许可证：MIT。原始版权声明保留在 [LICENSE](LICENSE) 中。

“C# Playground”名称及本分支 UI 为社区新增内容，不代表 Microsoft 官方品牌。


## 📚 相关文档

- [文档索引](documentation/README.zh-CN.md)
- [与上游项目的差异说明](MODIFICATIONS.zh-CN.md)
- [Differences from the upstream project](MODIFICATIONS.md)
- [安全策略](SECURITY.zh-CN.md)
