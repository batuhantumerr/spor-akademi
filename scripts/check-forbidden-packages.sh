#!/usr/bin/env bash
# Fails when a forbidden NuGet package is referenced, directly or transitively (CONTRIBUTING §8,
# DEC-048, DEC-061). A package counts when its id is a forbidden name or starts with one followed by a dot
# (for example AutoMapper.Extensions.Microsoft.DependencyInjection). NuGet ids are case-insensitive.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

forbidden='MediatR|AutoMapper|FluentAssertions'

packages=$(dotnet package list --include-transitive --format json)

found=$(grep -ioE "\"id\": \"(${forbidden})(\.[^\"]*)?\"" <<< "$packages" | sort -fu || true)

if [[ -n $found ]]; then
  echo "Forbidden packages are referenced (CONTRIBUTING §8):" >&2
  sed 's/^/  /' <<< "$found" >&2
  exit 1
fi
echo "No forbidden packages."
