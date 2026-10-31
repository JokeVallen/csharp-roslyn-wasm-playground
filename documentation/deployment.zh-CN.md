# 静态部署指南

**简体中文** | [English](deployment.md)

游乐场是 Blazor WebAssembly SPA。部署即发布静态文件，并用正确的 base 路径、MIME 类型和 SPA fallback 托管它们。

真实的 .NET 8 / .NET 9 / .NET 10 多运行时布局见 [运行时版本](runtimes.zh-CN.md)。

## 环境要求

- 多运行时构建需要 .NET 8、.NET 9 和 .NET 10 SDK
- 静态托管或 Web 服务器，例如 GitHub Pages、Nginx、IIS，或支持 SPA 重写的 CDN

## 本地开发

```bash
dotnet restore
dotnet run --project CSharpCompilerWasm/CSharpCompilerWasm.csproj
```

HTTP 启动配置默认为 `http://localhost:5158`。

## 发布

```bash
dotnet publish CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f net10.0 -o ./publish
```

发布产物目录：

```text
publish/wwwroot/
```

## Base href 规则

`wwwroot/index.html` 中初始为：

```html
<base href="/" />
```

最终的 `<base href>` 必须与应用访问路径一致，并且以 `/` 结尾。

| 部署 URL | Base href |
| --- | --- |
| `https://example.com/` | `/` |
| `https://example.com/csharp-playground/` | `/csharp-playground/` |
| `https://csharp-roslyn-wasm-playground.pages.dev/` | `/` |
| `https://jokevallen.github.io/csharp-roslyn-wasm-playground/` | `/csharp-roslyn-wasm-playground/` |

辅助脚本会自动重写 base href：

```bash
./scripts/publish.sh /csharp-playground/ ./publish
# Windows：
./scripts/publish.ps1 -BaseHref /csharp-playground/ -OutputDir ./publish
```

## GitHub Pages

`.github/workflows/deploy.yml` 在推送到 `main` 时执行：

1. 使用 `dotnet publish` 发布应用；
2. 将 `<base href>` 重写为 `/csharp-roslyn-wasm-playground/`；
3. 创建空的 `.nojekyll`；
4. 将 `publish/wwwroot` 上传到 GitHub Pages。

`.nojekyll` 是必需的，否则 GitHub Pages 会忽略以下划线开头的目录，包括 `_framework`。

## Cloudflare Pages

`.github/workflows/deploy-cloudflare.yml` 会在每次推送到 `main` 时自动部署到 Cloudflare Pages。

首次运行前：

1. 在 Cloudflare 创建空的 Pages 项目，例如 `csharp-roslyn-wasm-playground`。
2. 在 GitHub 仓库添加 Secrets：
   - `CLOUDFLARE_API_TOKEN`：权限包含 `Account → Cloudflare Pages → Edit`。
   - `CLOUDFLARE_ACCOUNT_ID`：Cloudflare 账户 ID。
3. 如果项目名不同，可添加仓库变量 `CLOUDFLARE_PROJECT_NAME`。

工作流会：

1. 使用 `scripts/publish-all.sh "/" "publish/site"` 构建三个运行时版本；
2. 删除 `.br` / `.gz`、PDB、source map 和未使用的 Bootstrap 文件，避免 Cloudflare 直接上传的文件数量限制；
3. 写入 `_headers`，对带指纹资源使用长期缓存，对无哈希的 `blazor.webassembly.js` / `dotnet.js` 启动文件使用 `no-cache`；
4. 给无哈希启动文件 URL 加上应用版本查询参数，避免部署后浏览器/CDN 旧缓存继续引用已删除的哈希 DLL；
5. 通过 Wrangler 上传 `publish/site`。

Cloudflare 会自动压缩响应，因此不需要 Blazor 生成的 `.br` / `.gz` 文件。

## Nginx

可参考 [`deploy/nginx.conf.example`](../deploy/nginx.conf.example)。关键配置：

```nginx
types {
    application/octet-stream dll;
    application/wasm wasm;
    application/octet-stream dat;
    application/json json;
}

location /csharp-playground/ {
    try_files $uri $uri/ /csharp-playground/index.html;
}
```

- `try_files` 提供 SPA fallback，刷新 `/compiler` 时不会 404。
- `.dll`、`.wasm`、`.dat`、`.json` 需要正确的 MIME 类型。
- 不要添加 `X-Frame-Options: SAMEORIGIN` 或 `DENY`；应用需要嵌入外部博客。
- 如使用 CSP，请通过 `frame-ancestors` 允许博客域名。

## IIS

`docs/web.config` 提供了 IIS rewrite 示例。部署新构建产物时可采用同样规则：

- 托管子目录内容；
- 未匹配文件重写到 `index.html`；
- 移除 `X-Frame-Options`；
- 映射 `.dll`、`.wasm`、`.dat`、`.json`。

## 缓存建议

- `index.html` 和 `blazor.boot.json`：不缓存或短缓存；
- 带指纹的 `_framework/*` 以及 `.br`/`.gz` 压缩文件：长期 immutable 缓存。

## 验收清单

1. `index.html` 正常加载，且 `<base href>` 与部署路径一致。
2. `_framework/blazor.boot.json` 返回 200。
3. `_framework/*.dll` 和/或 `.wasm` 资源返回 200，MIME 类型可接受。
4. 刷新 `/compiler` 等路由地址时应用仍能加载。
5. `Console.WriteLine("Hello");` 能编译并输出 `Hello`。
6. 页面能嵌入目标博客的 iframe。

## 常见问题

| 现象 | 可能原因 |
| --- | --- |
| 部署后白屏 | `<base href>` 错误或 `_framework` 资源缺失 |
| 刷新 404 | 缺少 SPA fallback |
| GitHub Pages 下 `_framework` 404 | 缺少 `.nojekyll` |
| iframe 被阻止 | `X-Frame-Options` 或 CSP `frame-ancestors` |
| WASM/MIME 错误 | 服务器为 `.wasm` 返回了错误的 `Content-Type` |
| 本地正常但子路径不正常 | base href 与子路径不一致 |
