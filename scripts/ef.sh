#!/usr/bin/env bash
# Runs the dotnet-ef tool against the local database as spor_akademi_migrator (ADR-0011).
#
#   scripts/ef.sh --check-env                    checks the .env values; prints only variable names
#   scripts/ef.sh migrations add <Name>
#   scripts/ef.sh migrations script --idempotent
#   scripts/ef.sh database update
#
# .env is read as data, line by line; it is never sourced, so its values cannot run as shell code.
# Nothing in this script prints a value from .env: no `set -x`, no echo of variables, and dotnet-ef's
# --verbose output is refused.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."

fail() {
  echo "ef.sh: $1" >&2
  exit 1
}

# Sets the shell variable named $1 from the line "$1=value" in .env; the last matching line wins.
read_env() {
  local line
  while IFS= read -r line || [[ -n $line ]]; do
    line=${line%$'\r'}
    if [[ $line == "$1="* ]]; then
      printf -v "$1" '%s' "${line#*=}"
    fi
  done < .env
}

# Passwords end up in connection strings, where characters such as ; or quotes change their meaning.
is_alphanumeric() {
  [[ ${!1:-} =~ ^[A-Za-z0-9]+$ ]]
}

is_port() {
  [[ ${!1:-} =~ ^[0-9]+$ ]]
}

[[ -f .env ]] || fail "missing .env; create it with: cp .env.example .env"

if [[ ${1:-} == "--check-env" ]]; then
  invalid=()
  for name in POSTGRES_SUPERUSER_PASSWORD SPOR_AKADEMI_MIGRATOR_PASSWORD SPOR_AKADEMI_APP_PASSWORD SEQ_ADMIN_PASSWORD; do
    read_env "$name"
    is_alphanumeric "$name" || invalid+=("$name")
  done
  read_env POSTGRES_HOST_PORT
  POSTGRES_HOST_PORT=${POSTGRES_HOST_PORT:-5432}
  is_port POSTGRES_HOST_PORT || invalid+=("POSTGRES_HOST_PORT")

  if ((${#invalid[@]} > 0)); then
    echo "Invalid or missing (passwords: letters and digits only; port: digits only):" >&2
    printf '  %s\n' "${invalid[@]}" >&2
    exit 1
  fi
  echo "OK"
  exit 0
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

read_env SPOR_AKADEMI_MIGRATOR_PASSWORD
read_env POSTGRES_HOST_PORT
POSTGRES_HOST_PORT=${POSTGRES_HOST_PORT:-5432}
is_alphanumeric SPOR_AKADEMI_MIGRATOR_PASSWORD ||
  fail "SPOR_AKADEMI_MIGRATOR_PASSWORD must contain only letters and digits; run: scripts/ef.sh --check-env"
is_port POSTGRES_HOST_PORT || fail "POSTGRES_HOST_PORT must contain only digits"

extra=()
if [[ $1 == "migrations" && $2 == "add" ]]; then
  extra+=(--output-dir Persistence/Migrations)
fi

ConnectionStrings__Migrator="Host=127.0.0.1;Port=${POSTGRES_HOST_PORT};Database=spor_akademi;Username=spor_akademi_migrator;Password=${SPOR_AKADEMI_MIGRATOR_PASSWORD}" \
  exec dotnet ef "$@" "${extra[@]}" \
  --project src/SporAkademi.Infrastructure \
  --startup-project src/SporAkademi.Api
