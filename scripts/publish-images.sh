#!/usr/bin/env bash
# Pushes local images to a registry under one tag, then pulls each tag back and checks that it points to the
# manifest that was pushed (P0-31, DEC-089). CI publishes to GHCR with this script; locally it runs against
# a throwaway registry container (see CLAUDE.md).
#
# Usage: scripts/publish-images.sh <registry-prefix> <tag> <local-image>...
#   scripts/publish-images.sh ghcr.io/owner "$GITHUB_SHA" spor-akademi-api:ci spor-akademi-admin:ci
# pushes ghcr.io/owner/spor-akademi-api:<tag> and ghcr.io/owner/spor-akademi-admin:<tag>.
#
# The caller logs in to the registry first. The digests are compared from the push and pull output rather
# than from image IDs, because an image ID means something different in the classic and containerd image
# stores.
set -euo pipefail

prefix=${1:?usage: scripts/publish-images.sh <registry-prefix> <tag> <local-image>...}
tag=${2:?usage: scripts/publish-images.sh <registry-prefix> <tag> <local-image>...}
shift 2
(($# > 0)) || { echo "publish: no images given" >&2; exit 1; }

fail() {
  echo "publish: $1" >&2
  exit 1
}

# Only the commit SHA is used as a tag (DEC-089); a movable tag such as "latest" is rejected.
[[ $tag =~ ^[0-9a-f]{40}$ ]] || fail "tag must be a full 40-character commit SHA, got '$tag'"

for image in "$@"; do
  name=${image%%:*}
  remote="${prefix}/${name}:${tag}"

  docker image inspect "$image" > /dev/null 2>&1 || fail "local image $image not found"
  docker tag "$image" "$remote"

  # Without a terminal, docker push prints a line per layer and progress update; only the digest is kept.
  push_output=$(docker push "$remote") || { echo "$push_output" >&2; fail "$remote: push failed"; }
  pushed=$(grep -oE 'digest: sha256:[0-9a-f]{64}' <<< "$push_output" | tail -n 1 | cut -d ' ' -f 2) \
    || fail "$remote: no digest in the push output"

  # Remove the local tag so the pull below asks the registry instead of reusing the local reference.
  docker image rm "$remote" > /dev/null
  pull_output=$(docker pull "$remote") || fail "$remote: pull failed"
  pulled=$(grep -oE 'Digest: sha256:[0-9a-f]{64}' <<< "$pull_output" | tail -n 1 | cut -d ' ' -f 2) \
    || fail "$remote: no digest in the pull output"

  [[ $pulled == "$pushed" ]] || fail "$remote: registry returns $pulled, but $pushed was pushed"
  echo "$remote: $pushed (pushed and pulled back)"
done
echo "OK"
