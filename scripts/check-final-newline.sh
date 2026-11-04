#!/usr/bin/env bash
# Fails when a tracked text file does not end with a newline. The rule is already in .editorconfig
# (insert_final_newline = true); this check enforces it in CI (P0-28). Binary files are skipped because
# git grep -I ignores them.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

missing=()
# Exception (P0-38): files named exactly packages.lock.json. NuGet writes them without a final newline and
# rewrites them on every --force-evaluate; they must not be edited by hand, so they are left out of this
# check instead of fighting the tool.
while IFS= read -r -d '' file; do
  # Command substitution drops a trailing newline, so the last byte reads as empty only when it is one.
  if [[ -n $(tail -c 1 "$file") ]]; then
    missing+=("$file")
  fi
done < <(git grep -I -l -z '' -- . ':(exclude,glob)**/packages.lock.json')

if ((${#missing[@]} > 0)); then
  echo "These files must end with a newline:" >&2
  printf '  %s\n' "${missing[@]}" >&2
  exit 1
fi
echo "All text files end with a newline."
