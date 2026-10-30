param(
    [string]$BaseHref = "/csharp-playground/",
    [string]$OutputDir = "publish",
    [string]$Framework = "net10.0"
)

$ErrorActionPreference = "Stop"

if (-not $BaseHref.EndsWith("/")) {
    $BaseHref += "/"
}

dotnet publish ./CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f $Framework -o $OutputDir
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$indexHtml = Join-Path $OutputDir "wwwroot/index.html"
if (-not (Test-Path $indexHtml)) {
    throw "ERROR: $indexHtml not found after publish"
}

$content = Get-Content $indexHtml -Raw
$content = $content -replace '<base href="[^"]*" />', "<base href=`"$BaseHref`" />"
Set-Content -Path $indexHtml -Value $content -NoNewline

if (-not (Select-String -Path $indexHtml -Pattern ([regex]::Escape("<base href=`"$BaseHref`" />")) -Quiet)) {
    throw "ERROR: base href rewrite failed"
}

# Required on GitHub Pages and similar Jekyll-backed hosts so _framework is not ignored.
New-Item -ItemType File -Path (Join-Path $OutputDir "wwwroot/.nojekyll") -Force | Out-Null

Write-Host "Published to $OutputDir/wwwroot"
Write-Host "Base href: $BaseHref"
