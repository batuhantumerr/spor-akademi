#!/usr/bin/env bash
# Smoke test for the API image (P0-29): runs it without a database and checks what must hold in every
# environment. Usage: scripts/smoke-test-api-image.sh <image>
#
# - The image runs as a non-root user and contains neither .env nor the EF Core design-time assembly.
# - The API starts (PlatformRequirements found Europe/Istanbul and tr-TR).
# - /health/live is 200 and /health/ready is 503, because there is no database.
set -euo pipefail

image=${1:?usage: scripts/smoke-test-api-image.sh <image>}
port=18080
container=spor-akademi-api-smoke

fail() {
  echo "smoke test: $1" >&2
  if docker container inspect "$container" > /dev/null 2>&1; then
    echo "--- container log ---" >&2
    docker logs "$container" >&2 || true
  fi
  exit 1
}

cleanup() {
  docker rm --force "$container" > /dev/null 2>&1 || true
}
trap cleanup EXIT
cleanup

user=$(docker image inspect "$image" --format '{{.Config.User}}')
[[ -n $user && $user != root && $user != 0 && $user != 0:0 ]] || fail "image runs as root (User='$user')"
echo "User: $user"

files=$(docker create --name "$container" "$image" > /dev/null && docker export "$container" | tar -t)
docker rm "$container" > /dev/null
if grep -qE '(^|/)\.env$' <<< "$files"; then fail "image contains a .env file"; fi
if grep -q 'Microsoft.EntityFrameworkCore.Design.dll' <<< "$files"; then
  fail "image contains Microsoft.EntityFrameworkCore.Design.dll"
fi
echo "No .env and no EF Core design-time assembly in the image."

# A syntactically valid connection string to a host that does not exist; it holds no secret.
docker run --detach --name "$container" --publish "127.0.0.1:${port}:8080" \
  --env 'ConnectionStrings__App=Host=db.invalid;Database=spor_akademi;Username=spor_akademi_app' \
  "$image" > /dev/null

status() {
  curl --silent --output /dev/null --write-out '%{http_code}' --max-time 10 "http://127.0.0.1:${port}$1" || true
}

for _ in $(seq 1 30); do
  [[ $(docker inspect --format '{{.State.Running}}' "$container") == true ]] || fail "container exited"
  [[ $(status /health/live) == 200 ]] && break
  sleep 1
done

live=$(status /health/live)
ready=$(status /health/ready)
echo "/health/live: $live, /health/ready: $ready"
[[ $live == 200 ]] || fail "/health/live returned $live, expected 200"
[[ $ready == 503 ]] || fail "/health/ready returned $ready, expected 503 without a database"
echo "OK"
