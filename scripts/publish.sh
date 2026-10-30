#!/usr/bin/env bash
set -euo pipefail

# Usage:
#   ./scripts/publish.sh [base-href] [output-dir] [framework]
# Example:
#   ./scripts/publish.sh /csharp-playground/ ./publish net10.0
#
# The trailing slash in base-href is mandatory for Blazor.

BASE_HREF="${1:-/csharp-playground/}"
OUT_DIR="${2:-publish}"
FRAMEWORK="${3:-net10.0}"

case "$BASE_HREF" in
  */) ;;
  *) BASE_HREF="${BASE_HREF}/" ;;
esac

dotnet publish ./CSharpCompilerWasm/CSharpCompilerWasm.csproj -c Release -f "$FRAMEWORK" -o "$OUT_DIR"

INDEX_HTML="$OUT_DIR/wwwroot/index.html"
if [ ! -f "$INDEX_HTML" ]; then
  echo "ERROR: $INDEX_HTML not found after publish" >&2
  exit 1
fi

# Rewrite Blazor's <base href="..."> for the final deployment path.
sed -i "s|<base href=\"[^\"]*\" />|<base href=\"${BASE_HREF}\" />|" "$INDEX_HTML"
grep -q "<base href=\"${BASE_HREF}\" />" "$INDEX_HTML"

# Required on GitHub Pages and similar Jekyll-backed hosts so _framework is not ignored.
touch "$OUT_DIR/wwwroot/.nojekyll"

echo "Published to $OUT_DIR/wwwroot"
echo "Base href: $BASE_HREF"
