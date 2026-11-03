#!/usr/bin/env bash
# Runs the API locally against the Docker Compose database as spor_akademi_app (NFR-SEC-10).
#
#   scripts/run-api.sh
#
# .env is read through scripts/lib/env.sh and its values are never printed. The connection string is
# passed only to the API process as ConnectionStrings__App.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/.."
# shellcheck source=lib/env.sh
. scripts/lib/env.sh

(($# == 0)) || fail "takes no arguments"
require_env_file

build_connection_string connection_string spor_akademi_app SPOR_AKADEMI_APP_PASSWORD

ConnectionStrings__App=$connection_string \
  exec dotnet run --project src/SporAkademi.Api --launch-profile http
