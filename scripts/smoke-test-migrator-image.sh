#!/usr/bin/env bash
# Smoke test for the migrator image (P0-31, DEC-089, ADR-0011): runs the migration bundle against a
# throwaway PostgreSQL whose roles are created exactly as in the local environment.
# Usage: scripts/smoke-test-migrator-image.sh <image>
#
# - The image runs as a non-root user and contains neither .env nor a .NET SDK.
# - The bundle applies every migration as spor_akademi_migrator, and the migration IDs it records are the
#   same as those in the image's migrations.sql: the reviewed SQL and the bundle come from the same build.
# - spor_akademi_app can use the tables the bundle created, but not the migrations history (P0-27).
# - A second run applies nothing; a wrong password fails. The output never contains a password.
#
# The passwords are random for each run, passed to containers by variable name only (never on a command
# line) and never printed.
set -euo pipefail
# Git Bash on Windows would otherwise rewrite container paths such as /migrations into Windows paths.
export MSYS_NO_PATHCONV=1

image=${1:?usage: scripts/smoke-test-migrator-image.sh <image>}
postgres_image=postgres:18.6-trixie@sha256:3a4264520e121176619f54ff867d12774bc81e95e7b84db58d63bccdb6144b81
network=spor-akademi-migrator-smoke
db=spor-akademi-migrator-smoke-db
probe=spor-akademi-migrator-smoke-probe

cd "$(dirname "${BASH_SOURCE[0]}")/.."

POSTGRES_PASSWORD=$(openssl rand -hex 24)
SPOR_AKADEMI_MIGRATOR_PASSWORD=$(openssl rand -hex 24)
SPOR_AKADEMI_APP_PASSWORD=$(openssl rand -hex 24)
wrong_password=$(openssl rand -hex 24)
export POSTGRES_PASSWORD SPOR_AKADEMI_MIGRATOR_PASSWORD SPOR_AKADEMI_APP_PASSWORD

fail() {
  echo "smoke test: $1" >&2
  exit 1
}

cleanup() {
  docker rm --force "$db" "$probe" > /dev/null 2>&1 || true
  docker network rm "$network" > /dev/null 2>&1 || true
}
trap cleanup EXIT
cleanup

# Fails if the given output contains any of this run's passwords.
assert_no_password() {
  local output=$1 secret
  for secret in "$POSTGRES_PASSWORD" "$SPOR_AKADEMI_MIGRATOR_PASSWORD" "$SPOR_AKADEMI_APP_PASSWORD" \
    "$wrong_password"; do
    if grep -qF "$secret" <<< "$output"; then fail "$2: the output contains a password"; fi
  done
}

# Runs the bundle as spor_akademi_migrator with the given password; prints its output, returns its status.
run_bundle() {
  ConnectionStrings__Migrator="Host=$db;Database=spor_akademi;Username=spor_akademi_migrator;Password=$1" \
    docker run --rm --network "$network" --env ConnectionStrings__Migrator "$image" 2>&1
}

psql_as_postgres() {
  docker exec "$db" psql --username postgres --dbname spor_akademi --no-align --tuples-only \
    --set ON_ERROR_STOP=1 --command "$1"
}

# --- The image itself ---
user=$(docker image inspect "$image" --format '{{.Config.User}}')
[[ -n $user && $user != root && $user != 0 && $user != 0:0 ]] || fail "image runs as root (User='$user')"
echo "User: $user"

files=$(docker create --name "$probe" "$image" > /dev/null && docker export "$probe" | tar -t)
if grep -qE '(^|/)\.env$' <<< "$files"; then fail "image contains a .env file"; fi
if grep -qE '(^|/)sdk/[0-9]' <<< "$files"; then fail "image contains a .NET SDK"; fi
echo "No .env and no .NET SDK in the image."

sql=$(docker cp "$probe:/migrations/migrations.sql" - | tar -xO) || fail "image has no /migrations/migrations.sql"
docker rm "$probe" > /dev/null
sql_ids=$(grep -oE '"migration_id" = '"'"'[^'"'"']+'"'" <<< "$sql" | cut -d "'" -f 2 | sort -u)
[[ -n $sql_ids ]] || fail "migrations.sql contains no migration IDs"
echo "migrations.sql: $(wc -l <<< "$sql_ids") migrations"

# --- A throwaway database with the same roles as the local environment (docker/postgres/initdb) ---
docker network create "$network" > /dev/null
docker create --name "$db" --network "$network" \
  --env POSTGRES_PASSWORD --env POSTGRES_DB=spor_akademi \
  --env SPOR_AKADEMI_MIGRATOR_PASSWORD --env SPOR_AKADEMI_APP_PASSWORD \
  "$postgres_image" > /dev/null
docker cp docker/postgres/initdb/10-create-roles.sh "$db:/docker-entrypoint-initdb.d/10-create-roles.sh"
docker start "$db" > /dev/null

# TCP on purpose: during initialisation the server listens on the socket only (compose.yaml).
ready=false
for _ in $(seq 1 60); do
  if docker exec "$db" pg_isready --host 127.0.0.1 --username postgres --dbname spor_akademi > /dev/null 2>&1; then
    ready=true
    break
  fi
  sleep 1
done
[[ $ready == true ]] || fail "PostgreSQL did not become ready"

# --- First run: every migration is applied ---
output=$(run_bundle "$SPOR_AKADEMI_MIGRATOR_PASSWORD") || { echo "$output" >&2; fail "bundle failed"; }
assert_no_password "$output" "first run"
db_ids=$(psql_as_postgres 'SELECT migration_id FROM public.__ef_migrations_history ORDER BY 1')
[[ $db_ids == "$sql_ids" ]] || fail "applied migrations differ from migrations.sql: [$db_ids] vs [$sql_ids]"
echo "First run: applied the $(wc -l <<< "$db_ids") migrations in migrations.sql."

# --- Privileges: the application role can use the tables, but not the migrations history ---
privileges=$(psql_as_postgres "
  SELECT table_schema || '.' || table_name || ' ' ||
         has_table_privilege('spor_akademi_app', format('%I.%I', table_schema, table_name), 'SELECT')
  FROM information_schema.tables
  WHERE table_type = 'BASE TABLE' AND table_schema NOT IN ('pg_catalog', 'information_schema')
  ORDER BY 1")
grep -qx 'public.__ef_migrations_history false' <<< "$privileges" \
  || fail "spor_akademi_app can read the migrations history"
# Every other table must be readable. There are none until the modules add theirs (the first migration is
# empty), so this check starts to bite with the first module table.
app_tables=$(grep -v '^public.__ef_migrations_history ' <<< "$privileges" || true)
if grep -q ' false$' <<< "$app_tables"; then fail "spor_akademi_app cannot read: $(grep ' false$' <<< "$app_tables")"; fi
echo "spor_akademi_app: reads all $(grep -c . <<< "$app_tables" || true) other tables, not the migrations history."

# --- Second run: nothing left to apply ---
output=$(run_bundle "$SPOR_AKADEMI_MIGRATOR_PASSWORD") || { echo "$output" >&2; fail "second run failed"; }
assert_no_password "$output" "second run"
[[ $(psql_as_postgres 'SELECT migration_id FROM public.__ef_migrations_history ORDER BY 1') == "$db_ids" ]] \
  || fail "second run changed the migrations history"
echo "Second run: nothing applied."

# --- Wrong password: the bundle fails and does not print it ---
if output=$(run_bundle "$wrong_password"); then fail "bundle succeeded with a wrong password"; fi
assert_no_password "$output" "wrong password run"
echo "Wrong password: the bundle failed."
echo "OK"
