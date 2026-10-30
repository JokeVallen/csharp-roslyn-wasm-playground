# Console API 参考

**简体中文** | [English](console-api.md)

游乐场在编译时注入一个最小化的 `Console` 类来替代 `System.Console`，用于捕获输出并通过浏览器 UI 支持交互式行输入。

## 支持的成员

### `Console.Write`

| 签名 | 说明 |
| --- | --- |
| `Write(string value)` | 输出文本；`null` 视为空字符串。 |
| `Write(char value)` | 输出一个字符。 |
| `Write(object value)` | 输出 `value?.ToString()`。 |
| `Write(int value)` | 输出整数。 |
| `Write(long value)` | 输出长整数。 |
| `Write(float value)` | 按当前区域格式输出。 |
| `Write(double value)` | 按当前区域格式输出。 |
| `Write(decimal value)` | 按当前区域格式输出。 |
| `Write(bool value)` | 输出 `True` 或 `False`。 |

### `Console.WriteLine`

| 签名 | 说明 |
| --- | --- |
| `WriteLine()` | 输出一个换行。 |
| `WriteLine(string value)` | 输出文本并换行。 |
| `WriteLine(char value)` | 输出一个字符并换行。 |
| `WriteLine(object value)` | 输出 `value?.ToString()` 并换行。 |
| `WriteLine(int value)` | 输出整数并换行。 |
| `WriteLine(long value)` | 输出长整数并换行。 |
| `WriteLine(float value)` | 输出浮点数并换行。 |
| `WriteLine(double value)` | 输出双精度数并换行。 |
| `WriteLine(decimal value)` | 输出十进制数并换行。 |
| `WriteLine(bool value)` | 输出 `True` 或 `False` 并换行。 |

### `Console.ReadLine`

```csharp
string name = Console.ReadLine();
```

返回用户在游乐场控制台输入下一行。

实现细节：当输入队列为空时，它会抛出 `InvalidOperationException("INPUT_REQUIRED")`。执行器捕获该标记，请求 UI 输入一行，然后用已收集的输入重新开始执行。因此用户输入后，程序可能会看起来从头再运行一次。

## 不支持的成员

注入的类刻意保持精简，以下常见 `System.Console` 成员**不可用**：

- `Read()`、`ReadKey()`、`ReadLineAsync()`
- `WriteLine(string format, params object[] args)` 重载
- `ForegroundColor`、`BackgroundColor`、`ResetColor()`
- `Out`、`Error`、`In`
- `OpenStandardInput`、`SetOut`、`SetError`
- 光标定位及其他终端 API

字符串插值仍然可用，因为它在调用 `WriteLine` 前已经生成字符串：

```csharp
Console.WriteLine($"Hello, {name}!");
```

## 示例

### 基本输出

```csharp
Console.WriteLine("Hello");
Console.Write("C#");
Console.WriteLine(" Playground");
```

输出：

```text
Hello
C# Playground
```

### 交互式输入

```csharp
Console.Write("What is your name? ");
string name = Console.ReadLine();
Console.WriteLine($"Hello, {name}!");
```

第一次执行到 `ReadLine()` 时，UI 请求输入，随后程序带着该输入重新开始执行。

### 循环与计算

```csharp
for (int i = 1; i <= 3; i++)
{
    Console.WriteLine($"Count: {i}");
}

Console.Write("Enter a number: ");
string input = Console.ReadLine();
if (int.TryParse(input, out int number))
{
    Console.WriteLine(number * 2);
}
```

## 内部宿主成员

以下成员供 Blazor 执行器使用，不属于用户可见 API：

- `GetCapturedOutput()`
- `ClearOutput()`
- `ProvideInput(string input)`

## 源码位置

生成的类位于：

`CSharpCompilerWasm/ConsoleReplacementCode.cs`

完整执行流程与限制见 [浏览器 C# 运行时](browser-runtime.zh-CN.md)。
