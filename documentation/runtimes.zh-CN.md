# 运行时版本

**简体中文** | [English](runtimes.md)

游乐场可以部署三个真实的 Blazor WebAssembly 运行时构建：

| 运行时 | URL | Blazor WASM 包 | 可用的编译目标 |
| --- | --- | --- | --- |
| .NET 8 | `/net8/` | 8.0.33 | `net8.0` |
| .NET 9 | `/net9/` | 9.0.22 | `net8.0`、`net9.0` |
| .NET 10 | `/net10/` | 10.0.14 | `net8.0`、`net9.0`、`net10.0` |

站点根路径默认使用 .NET 10 构建，以兼容已有链接。

## 选择运行时

### 直接访问 URL

```text
https://csharp-roslyn-wasm-playground.pages.dev/net8/
https://csharp-roslyn-wasm-playground.pages.dev/net9/
https://csharp-roslyn-wasm-playground.pages.dev/net10/
```

### iframe

```html
<iframe
  src="https://csharp-roslyn-wasm-playground.pages.dev/net8/?theme=light&accent=blue"
  width="100%" height="600" frameborder="0" loading="lazy"
  title=".NET 8 上的 C# Playground"></iframe>
```

### `runtime` 重定向参数

根页面支持 `runtime` 查询参数，并跳转到对应构建：

```text
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net8.0
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net9.0
https://csharp-roslyn-wasm-playground.pages.dev/?runtime=net10.0
```

这是页面跳转，不是页面内运行时切换。已经加载的 Blazor WebAssembly 页面无法替换其打包的 `dotnet.wasm`。

## 为什么必须分开构建

Blazor WebAssembly 在发布时把运行时（`dotnet.wasm`）和启动配置一起打包，运行时不是可以通过下拉框替换的脚本。

因此每个运行时都需要独立的 `dotnet publish` 产物：

```bash
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net8.0 -o publish/net8
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net9.0 -o publish/net9
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net10.0 -o publish/net10
```

仓库提供的多运行时脚本会自动完成构建并重写各自的 base href：

```bash
./scripts/publish-all.sh /csharp-playground/ ./publish/site
# Windows：
./scripts/publish-all.ps1 -RootBaseHref /csharp-playground/ -OutputDir ./publish/site
```

## 本地开发

安装需要测试的 SDK 后，用 `-f` 选择目标框架：

```bash
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net8.0
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net9.0
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj -f net10.0
```

启动地址仍为 `http://localhost:5158`，除非修改 `Properties/launchSettings.json`。

## 部署结构

GitHub Pages artifact 结构如下：

```text
/
  index.html            # 默认 .NET 10 构建，base href 为 /
  _framework/
  net8/
    index.html          # base href /csharp-playground/net8/
    _framework/
  net9/
    index.html          # base href /csharp-playground/net9/
    _framework/
  net10/
    index.html          # base href /csharp-playground/net10/
    _framework/
  .nojekyll
```

每个运行时都有独立负载和缓存命名空间，用户只会下载自己打开的版本。

## 取舍

- 三个发布产物会增加 CI 时间和存储占用。
- .NET 8 构建只能选择 .NET 8 引用程序集。
- .NET 9 构建可以选择 .NET 8 和 .NET 9。
- .NET 10 构建可以选择 .NET 8、.NET 9 和 .NET 10。
- .NET 6/7 已 EOL，因此不纳入运行时矩阵。

每个运行时内部的编译目标行为见 [目标框架选择](target-frameworks.zh-CN.md)。
