#!/usr/bin/env bash
set -euo pipefail

# Build real .NET 8, .NET 9, and .NET 10 runtime variants and lay them out as:
#   <output>/          -> default .NET 10 build
#   <output>/net8/
#   <output>/net9/
#   <output>/net10/
#
# Usage:
#   ./scripts/publish-all.sh [root-base-href] [output-dir]
# Example:
#   ./scripts/publish-all.sh /csharp-playground/ ./publish/site

ROOT_BASE="${1:-/csharp-playground/}"
OUT_DIR="${2:-publish/site}"

case "$ROOT_BASE" in
  */) ;;
  *) ROOT_BASE="${ROOT_BASE}/" ;;
esac

rm -rf "$OUT_DIR"
mkdir -p "$OUT_DIR"

build_runtime() {
  local tfm="$1"
  local dir="$2"
  local base="${ROOT_BASE}${dir}/"
  local work="publish/.runtime-${dir}"

  rm -rf "$work"
  dotnet publish ./CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f "$tfm" -o "$work"
  sed -i "s|<base href=\"[^\"]*\" />|<base href=\"${base}\" />|" "$work/wwwroot/index.html"
  mkdir -p "$OUT_DIR/$dir"
  cp -r "$work/wwwroot/." "$OUT_DIR/$dir/"
}

build_runtime net8.0 net8
build_runtime net9.0 net9
build_runtime net10.0 net10

# Keep the historical root URL working by using .NET 10 as the default build.
cp -r publish/.runtime-net10/wwwroot/. "$OUT_DIR/"
sed -i "s|<base href=\"[^\"]*\" />|<base href=\"${ROOT_BASE}\" />|" "$OUT_DIR/index.html"

touch "$OUT_DIR/.nojekyll"

# Cloudflare Pages and Netlify compress responses themselves, and Cloudflare's
# direct upload has a file-count limit. Drop precompressed copies and debug data.
find "$OUT_DIR" -type f \( -name '*.br' -o -name '*.gz' -o -name '*.pdb' -o -name '*.map' \) -delete
if [ -d "$OUT_DIR/lib/bootstrap" ]; then
  find "$OUT_DIR/lib/bootstrap" -type f ! -name 'bootstrap.min.css' -delete
fi

cat > "$OUT_DIR/_headers" <<'EOF'
/_framework/*
  Cache-Control: public, max-age=31536000, immutable

/*
  Cache-Control: public, max-age=0, must-revalidate
EOF

echo "Published multi-runtime site to $OUT_DIR"
echo "Root base href: $ROOT_BASE"
echo "Runtime URLs: ${ROOT_BASE}net8/ ${ROOT_BASE}net9/ ${ROOT_BASE}net10/"
