# Shared helpers for scripts that use .env (scripts/ef.sh, scripts/run-api.sh). Sourced, not executed.
#
# .env is read as data, line by line; it is never sourced, so its values cannot run as shell code.
# Nothing here prints a value from .env; errors name only the variable. Callers must run from the
# repository root and must not use `set -x`.

fail() {
  echo "$(basename "$0"): $1" >&2
  exit 1
}

require_env_file() {
  [[ -f .env ]] || fail "missing .env; create it with: cp .env.example .env"
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

# Checks every value in .env and exits; prints only "OK" or the names of invalid variables.
check_env() {
  local invalid=() name
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
}

# Sets the variable named $1 to a connection string for the local database as user $2, whose password
# is read from the .env variable named $3. The value is assigned, never printed.
build_connection_string() {
  local target=$1 username=$2 password_variable=$3
  read_env "$password_variable"
  read_env POSTGRES_HOST_PORT
  POSTGRES_HOST_PORT=${POSTGRES_HOST_PORT:-5432}
  is_alphanumeric "$password_variable" ||
    fail "$password_variable must contain only letters and digits; run: scripts/ef.sh --check-env"
  is_port POSTGRES_HOST_PORT || fail "POSTGRES_HOST_PORT must contain only digits"

  printf -v "$target" '%s' \
    "Host=127.0.0.1;Port=${POSTGRES_HOST_PORT};Database=spor_akademi;Username=${username};Password=${!password_variable}"
}
