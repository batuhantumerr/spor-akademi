#!/usr/bin/env bash
# Smoke test for the admin app image (P0-29). Usage: scripts/smoke-test-admin-image.sh <image>
#
# - The image runs as a non-root user and contains no .env file.
# - / serves index.html with Cache-Control: no-cache.
# - A client-side route such as /groups/1 also serves index.html (SPA fallback).
# - The Server header does not reveal the nginx version (server_tokens off).
# - Hashed bundles are cached as immutable; a missing bundle is 404, not index.html.
set -euo pipefail

image=${1:?usage: scripts/smoke-test-admin-image.sh <image>}
port=18081
container=spor-akademi-admin-smoke

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
echo "No .env in the image."

docker run --detach --name "$container" --publish "127.0.0.1:${port}:8080" "$image" > /dev/null
base="http://127.0.0.1:${port}"

for _ in $(seq 1 30); do
  curl --silent --output /dev/null --max-time 2 "$base/" && break
  sleep 1
done

check() {
  local path=$1 expected_status=$2 expected_text=$3
  local response
  response=$(curl --silent --include --max-time 10 "$base$path") || fail "$path: request failed"
  local status
  status=$(head -n 1 <<< "$response" | awk '{print $2}')
  [[ $status == "$expected_status" ]] || fail "$path returned $status, expected $expected_status"
  grep -qiF "$expected_text" <<< "$response" || fail "$path: response does not contain '$expected_text'"
  echo "$path: $status, contains '$expected_text'"
}

check / 200 "Cache-Control: no-cache"
check / 200 "<app-root>"
check /groups/1 200 "<app-root>"

# server_tokens off: the Server header names nginx but not its version.
server=$(curl --silent --head --max-time 10 "$base/" | grep -i '^server:' | tr -d '\r') || fail "/: no Server header"
[[ $server =~ ^[Ss]erver:\ nginx$ ]] || fail "/: Server header reveals more than 'nginx' ($server)"
echo "/: $server"

bundle=$(curl --silent --max-time 10 "$base/" | grep -oE 'main-[A-Za-z0-9]+\.js' | head -n 1)
[[ -n $bundle ]] || fail "index.html references no main-<hash>.js bundle"
check "/$bundle" 200 "immutable"
check /main-doesnotexist.js 404 "404"
echo "OK"
