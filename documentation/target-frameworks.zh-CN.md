# 目标框架选择

**简体中文** | [English](target-frameworks.md)

每个运行时构建可以使用一组或多组引用程序集（取决于构建版本，支持 .NET 8、.NET 9 和/或 .NET 10）编译用户代码。这是**编译期目标框架**切换，不是 WebAssembly 运行时切换。

## 切换的内容

| 目标 | 引用包 | 编译期可用的 API |
| --- | --- | --- |
| `net9.0`（默认） | `Basic.Reference.Assemblies.Net90` | .NET 9 API |
| `net8.0` | `Basic.Reference.Assemblies.Net80` | .NET 8 API |
| `net10.0` | `Basic.Reference.Assemblies.Net100` | .NET 10 API（仅 .NET 10 运行时构建可用） |

每个构建运行在自己的 Blazor 运行时上：`/net8/` 使用 .NET 8，`/net9/` 使用 .NET 9，`/net10/` 使用 .NET 10。编译目标会阻止 Roslyn 使用所选引用集之外的 API，但生成的程序集仍由当前页面所在的运行时执行。

| 运行时构建 | 可用编译目标 |
| --- | --- |
| `/net8/` | `net8.0` |
| `/net9/` | `net8.0`、`net9.0` |
| `/net10/` | `net8.0`、`net9.0`、`net10.0` |

## 如何选择目标

### 界面选择

编辑器工具栏提供了一个 **目标 / Target** 下拉框，可选 `.NET 9` 和 `.NET 8`。

### URL 参数

```text
?target=net8.0
```

`target` 不区分大小写，也接受 `net8` 作为别名。无法识别的值会回退到 `net9.0`。

示例：

```html
<iframe
  src="https://jokevallen.github.io/csharp-roslyn-wasm-playground/?target=net8.0&theme=light&accent=blue"
  width="100%" height="600" frameborder="0" loading="lazy"></iframe>
```

## 行为说明

- URL 参数决定初始目标框架。
- 修改下拉框会立即更新当前页面，但不会重写 iframe URL。
- 刷新页面后会重新使用 URL 中的目标（没有则使用 `net9.0`）。
- 切换目标只影响下一次编译，不影响已经运行中的代码。
- 不同目标下 API 可用性不同，编译诊断也可能不同。

## 重要限制

单个 Blazor WebAssembly 页面无法同时加载两个 .NET 运行时，本功能**不会**原地切换运行时。

需要真实 .NET 8 运行时请打开 `/net8/`；需要 .NET 10 请打开 `/net10/`。仓库会同时发布这三个构建，详见 [运行时版本](runtimes.zh-CN.md)。

## 实现位置

| 文件 | 改动 |
| --- | --- |
| `CSharpCompilerWasm/CSharpCompilerWasm.csproj` | 同时引用 `Basic.Reference.Assemblies.Net80` 和 `Basic.Reference.Assemblies.Net90` |
| `CSharpCompilerWasm/Pages/Compiler.razor` | `target` 查询参数、下拉框、按目标选择引用类型 |
| `documentation/iframe-style-api.zh-CN.md` | 相关的 iframe 嵌入参数说明 |

引用发现仍然使用 `typeof(MetadataReference).IsAssignableFrom(...)`，因为两个包暴露的都是 `PortableExecutableReference` 属性。
