param(
    [string]$RootBaseHref = "/csharp-playground/",
    [string]$OutputDir = "publish/site"
)

$ErrorActionPreference = "Stop"

if (-not $RootBaseHref.EndsWith("/")) {
    $RootBaseHref += "/"
}

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

function Publish-Runtime {
    param(
        [string]$Framework,
        [string]$Directory
    )

    $work = Join-Path "publish" ".runtime-$Directory"
    $baseHref = "$RootBaseHref$Directory/"

    if (Test-Path $work) {
        Remove-Item $work -Recurse -Force
    }
    dotnet publish ./CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f $Framework -o $work
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

    $indexHtml = Join-Path $work "wwwroot/index.html"
    $content = Get-Content $indexHtml -Raw
    $content = $content -replace '<base href="[^"]*" />', "<base href=`"$baseHref`" />"
    Set-Content -Path $indexHtml -Value $content -NoNewline

    $destination = Join-Path $OutputDir $Directory
    New-Item -ItemType Directory -Path $destination -Force | Out-Null
    Copy-Item -Path (Join-Path $work "wwwroot/*") -Destination $destination -Recurse -Force
}

Publish-Runtime -Framework "net8.0" -Directory "net8"
Publish-Runtime -Framework "net9.0" -Directory "net9"
Publish-Runtime -Framework "net10.0" -Directory "net10"

# Keep the historical root URL working by using .NET 10 as the default build.
Copy-Item -Path "publish/.runtime-net10/wwwroot/*" -Destination $OutputDir -Recurse -Force
$rootIndex = Join-Path $OutputDir "index.html"
$rootContent = Get-Content $rootIndex -Raw
$rootContent = $rootContent -replace '<base href="[^"]*" />', "<base href=`"$RootBaseHref`" />"
Set-Content -Path $rootIndex -Value $rootContent -NoNewline

New-Item -ItemType File -Path (Join-Path $OutputDir ".nojekyll") -Force | Out-Null

# Hosts such as Cloudflare Pages and Netlify compress responses themselves.
# Keep the upload small enough for Cloudflare's direct-upload file limit too.
Get-ChildItem $OutputDir -Recurse -File |
    Where-Object { $_.Extension -in ".br", ".gz", ".pdb", ".map" } |
    Remove-Item -Force

$bootstrapDir = Join-Path $OutputDir "lib/bootstrap"
if (Test-Path $bootstrapDir) {
    Get-ChildItem $bootstrapDir -Recurse -File |
        Where-Object { $_.Name -ne "bootstrap.min.css" } |
        Remove-Item -Force
}

$headers = @"
/_framework/*
  Cache-Control: public, max-age=31536000, immutable
"@
[System.IO.File]::WriteAllText((Join-Path $OutputDir "_headers"), $headers, [System.Text.UTF8Encoding]::new($false))

Write-Host "Published multi-runtime site to $OutputDir"
Write-Host "Root base href: $RootBaseHref"
Write-Host "Runtime URLs: ${RootBaseHref}net8/ ${RootBaseHref}net9/ ${RootBaseHref}net10/"
