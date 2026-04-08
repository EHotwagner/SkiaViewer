#!/usr/bin/env bash
set -euo pipefail

# Pack SkiaViewer with a timestamp-based prerelease version.
# Usage: ./pack-dev.sh <target-dir>
#   target-dir: Directory to place the .nupkg (required)

if [[ $# -lt 1 ]]; then
    echo "Usage: $0 <target-dir>" >&2
    echo "  target-dir: Directory to place the .nupkg (e.g., ~/projects/FSBarV1/nupkg)" >&2
    exit 1
fi

TARGET_DIR="$1"
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT="$SCRIPT_DIR/src/SkiaViewer/SkiaViewer.fsproj"
PACKAGE_ID="SkiaViewer"
SUFFIX="dev.$(date +%Y%m%dT%H%M%S)"

mkdir -p "$TARGET_DIR"

# Remove old dev versions of this package from target
rm -f "$TARGET_DIR"/${PACKAGE_ID}.*.nupkg

# Pack with timestamp suffix
dotnet pack "$PROJECT" --version-suffix "$SUFFIX" -o "$TARGET_DIR" -c Debug

echo "Packed ${PACKAGE_ID} with suffix ${SUFFIX} to ${TARGET_DIR}"
