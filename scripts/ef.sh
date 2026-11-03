#!/usr/bin/env bash
# Runs the dotnet-ef tool against the local database as spor_akademi_migrator (ADR-0011).
#
#   scripts/ef.sh --check-env                    checks the .env values; prints only variable names
#   scripts/ef.sh migrations add <Name>
#   scripts/ef.sh migrations script --idempotent
#   scripts/ef.sh database update
#
# .env is read through scripts/lib/env.sh and its values are never printed: no `set -x`, no echo of
# variables, and dotnet-ef's --verbose output is refused.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."
# shellcheck source=lib/env.sh
. scripts/lib/env.sh

require_env_file

if [[ ${1:-} == "--check-env" ]]; then
  check_env
fi

case "${1:-} ${2:-}" in
  "migrations add" | "migrations remove" | "migrations list" | "migrations script" | \
    "migrations has-pending-model-changes" | "database update") ;;
  *) fail "unsupported command '${*}'; see the usage at the top of scripts/ef.sh" ;;
esac

for arg in "$@"; do
  case $arg in
    -v | --verbose) fail "--verbose is not allowed; its output may include connection details" ;;
    --connection | --connection=*) fail "--connection is not allowed; migrations always run as spor_akademi_migrator" ;;
  esac
done

build_connection_string connection_string spor_akademi_migrator SPOR_AKADEMI_MIGRATOR_PASSWORD

migrations_dir=src/SporAkademi.Infrastructure/Persistence/Migrations
adding=false
extra=()
if [[ $1 == "migrations" && $2 == "add" ]]; then
  adding=true
  extra+=(--output-dir Persistence/Migrations)
  # Files written after this marker are the ones dotnet-ef creates or rewrites for the new migration.
  marker=$(mktemp)
  trap 'rm -f "$marker"' EXIT
fi

# The connection string exists only in dotnet-ef's environment, never in a file or in this shell's output.
ConnectionStrings__Migrator=$connection_string \
  dotnet ef "$@" "${extra[@]}" \
  --project src/SporAkademi.Infrastructure \
  --startup-project src/SporAkademi.Api

# dotnet-ef writes the new migration with CRLF, a BOM and a block-scoped namespace; bring only the files
# it just wrote to the repository style (.editorconfig) so `dotnet format --verify-no-changes` stays clean.
if [[ $adding == true ]]; then
  mapfile -t new_files < <(find "$migrations_dir" -name '*.cs' -newer "$marker")
  if ((${#new_files[@]} > 0)); then
    dotnet format src/SporAkademi.Infrastructure --include "${new_files[@]}"
  fi
fi
