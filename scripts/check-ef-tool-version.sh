#!/usr/bin/env bash
# Fails when the dotnet-ef tool version differs from the Microsoft.EntityFrameworkCore package version.
# Dependabot updates both files, but in separate pull requests; this check keeps them from drifting apart.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

manifest=.config/dotnet-tools.json
packages=Directory.Packages.props

tool_version=$(grep -A 1 '"dotnet-ef"' "$manifest" | grep -oE '"version": *"[^"]+"' | grep -oE '[0-9][^"]*' || true)
package_version=$(grep -oE '<PackageVersion Include="Microsoft\.EntityFrameworkCore" Version="[^"]+"' "$packages" |
  grep -oE 'Version="[^"]+"' | grep -oE '[0-9][^"]*' || true)

if [[ -z $tool_version || -z $package_version ]]; then
  echo "Could not read the versions (dotnet-ef in $manifest: '$tool_version'," \
    "Microsoft.EntityFrameworkCore in $packages: '$package_version')." >&2
  exit 1
fi

if [[ $tool_version != "$package_version" ]]; then
  echo "dotnet-ef $tool_version ($manifest) does not match Microsoft.EntityFrameworkCore" \
    "$package_version ($packages)." >&2
  exit 1
fi
echo "dotnet-ef and Microsoft.EntityFrameworkCore are both $tool_version."
