# 文档索引

**简体中文** | [English](README.md)

本目录提供按功能、API 和部署流程拆分的专项文档。`docs/` 是构建后生成的静态站点产物；这里的 Markdown 文件才是文档源文件。

## 文档列表

| 文档 | 内容 |
| --- | --- |
| [iframe 样式 API](iframe-style-api.zh-CN.md) | 主题、强调色、布局、chrome、密度、圆角和字号等 URL 参数 |
| [浏览器 C# 运行时](browser-runtime.zh-CN.md) | Roslyn 编译与 WebAssembly 执行流程、支持的代码结构及限制 |
| [Console API 参考](console-api.zh-CN.md) | 注入到用户代码中的 `Console` 子集 |
| [运行时版本](runtimes.zh-CN.md) | 真实的 .NET 8 / .NET 9 / .NET 10 部署与 URL 结构 |
| [目标框架选择](target-frameworks.zh-CN.md) | 使用 .NET 8 / .NET 9 / .NET 10 引用程序集编译用户代码 |
| [静态部署指南](deployment.zh-CN.md) | GitHub Pages、自托管发布、base href、SPA fallback 与 iframe 响应头 |
| [与上游项目的差异](../MODIFICATIONS.zh-CN.md) | 本分支相对 `MicrosoftLearning/c-sharp-minor` 的修改 |
| [项目 README](../README.zh-CN.md) | 功能特性、快速开始与仓库概览 |

## 语言约定

每个文档都有对应的 `*.zh-CN.md` 文件，并在顶部提供语言切换。修改行为或公开参数时，请同步更新中英文版本。

## 相关链接

- [安全策略](../SECURITY.zh-CN.md)
- [许可证](../LICENSE)
