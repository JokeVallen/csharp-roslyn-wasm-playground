# 浏览器 C# 运行时

**简体中文** | [English](browser-runtime.md)

本文说明游乐场如何完全在浏览器中编译和执行 C#、支持哪些代码结构，以及相关实现位置。

## 执行流程

1. 从 `<textarea>` 读取编辑器内容。
2. 在简化模式下，为内容添加缩进并包装为：

   ```csharp
   using System;

   public class Program
   {
       public static void Main()
       {
           // 编辑器内容
       }
   }
   ```

3. 提取 `using` 指令并放到生成代码的顶部。
4. 注入浏览器端可用的 `Console` 替代实现。
5. 将源码和目标框架发送给 Roslyn Web Worker。
6. Worker 加载所选的 `Basic.Reference.Assemblies.Net80` / `Net90` / `Net100` 引用集并创建 `CSharpCompilation`。
7. Worker 将程序集写入 `MemoryStream`，返回 base64 字节和诊断信息。
8. 主线程把 base64 程序集转发给 Worker 的 `RoslynWorker.Execute` 导出。
9. Worker 使用 `Assembly.Load(byte[])` 加载程序集，查找入口点（优先 `Assembly.EntryPoint`，其次 `Program.Main`）并调用。
10. `Console.ReadLine()` 抛出内部 `INPUT_REQUIRED` 哨兵；Worker 返回部分输出，主线程弹出输入框，随后 Worker 带累积输入重新执行。
11. 死循环或耗时程序只会阻塞 Worker；控制台 `Stop` 按钮会终止 Worker，下次运行时自动重建。

## 支持的代码结构

编辑器支持两种模式：

- **简化模式**：只写 `Main` 方法体。
- **完整代码模式**：写完整 C# 程序，包括 `using` 指令和包含 `Main` 的类。

推荐入口点：

```csharp
public class Program
{
    public static void Main()
    {
        Console.WriteLine("Hello");
    }
}
```

运行时识别 `Main()` 和 `Main(string[] args)`。当前执行器以同步方式调用入口方法，不保证 `async Task Main` 会被正确等待。

## Roslyn 引用

`Basic.Reference.Assemblies.Net80`、`Net90` 和 `Net100` 以便于嵌入的资源形式提供引用程序集。`Compiler.razor` 会查找所选包中 `References` 嵌套类型的成员。

`target` URL 参数或编辑器工具栏选择器用于选择当前运行时构建支持的编译目标。这只会改变编译期可用的 API；应用仍然在当前页面的 WebAssembly 运行时上执行程序集。详见 [目标框架选择](target-frameworks.zh-CN.md) 和 [运行时版本](runtimes.zh-CN.md)。

关键细节：这些属性返回 `PortableExecutableReference`，而不是精确的 `MetadataReference` 类型。本分支使用：

```csharp
typeof(MetadataReference).IsAssignableFrom(p.PropertyType)
```

如果使用精确类型相等判断，会加载到 0 个引用并导致编译失败。

项目还必须保留：

```xml
<WasmEnableWebcil>false</WasmEnableWebcil>
```

以便发布后的 `_framework` 保留标准 `.dll` 文件，方便检查和工具链使用。

## 编辑器辅助

- 输入后 120ms 显示本地关键字/成员提示，不会在每次按键时运行 Roslyn。
- 语义补全默认在停止输入 420ms 后自动触发；切换 `completion=manual` 或工具栏的“手动”模式后，可通过 `Ctrl + Space` 或 `✨ 提示` 按钮触发。
- 语义请求发送到 `worker.js`，在独立的 .NET WebAssembly 运行时中调用导出的 `RoslynWorker.GetCompletions`。
- Roslyn 编译、语义补全和用户代码执行全部运行在 Worker 中；主线程只负责编辑器事件、控制台输出、输入提示和 UI 动效。
- 使用 `↑` / `↓` 选择，`Enter` 或 `Tab` 插入，`Escape` 关闭。
- 顶部工具栏可直接切换深色/浅色/跟随系统主题和强调色，无需刷新。
- 编译执行期间，运行按钮会显示等待动画。

## 键盘快捷键

| 快捷键 | 操作 |
| --- | --- |
| `Ctrl + Enter` | 编译并运行 |
| `Ctrl + L` | 加载示例代码 |
| `Ctrl + Space` | 请求语义补全 |
| `Ctrl + K` | 清空控制台 |

## 输入处理

`Console.ReadLine()` 不会阻塞浏览器线程。执行器先用目前已收集的输入运行程序；当注入的 `Console` 抛出内部 `INPUT_REQUIRED` 标记时，UI 通过 JS Interop 再请求一行输入，然后用完整输入队列重新执行程序。

这意味着：

- 输入是交互式收集的，但程序可能从头执行多次；
- 下一次 `ReadLine()` 之前的副作用可能重复发生；
- 最终展示的是成功那次执行的输出。

## 限制

- 浏览器中不存在真正阻塞式控制台 I/O。
- 仅支持 [Console API 参考](console-api.zh-CN.md) 中列出的子集。
- 不能任意安装 NuGet 包；可用引用仅为应用打包的引用程序集。
- 除浏览器运行时提供的能力外，不能使用服务器端文件系统、进程、网络或操作系统 API。
- 长时间运行或死循环会阻塞 UI 线程。
- 本项目是教学工具，不是安全沙箱。

## 实现位置

| 文件 | 职责 |
| --- | --- |
| `CSharpCompilerWasm/Pages/Compiler.razor` | 编辑器、编译、emit、执行循环、诊断和 UI 参数 |
| `CSharpCompilerWasm/ConsoleReplacementCode.cs` | 注入的 `Console` 类源码 |
| `CSharpCompilerWasm/CSharpCompilerWasm.csproj` | 运行时与 Roslyn 包版本、Webcil 设置 |
| `CSharpCompilerWasm/wwwroot/index.html` | Blazor 宿主页与 `base href` |
