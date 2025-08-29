#!/usr/bin/env bash
set -euo pipefail

usage() {
  echo "Usage: $(basename "$0") <ShortcutName> \"Text zum Vorlesen\""
  exit 1
}

if [[ $# -lt 2 ]]; then
  usage
fi

SHORTCUT_NAME="$1"
shift
TEXT="$*"

# Text an den Shortcut pipen
echo "$TEXT" | shortcuts run "$SHORTCUT_NAME"