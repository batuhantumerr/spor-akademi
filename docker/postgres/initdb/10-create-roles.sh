#!/usr/bin/env bash
# Creates the database roles on the first start of an empty data directory (NFR-SEC-10).
#   spor_akademi_migrator: owns the database, runs migrations, creates schemas and tables.
#   spor_akademi_app:      used by the API; reads and writes data but cannot change the schema.
# Passwords are passed as psql variables and quoted with :'...', never pasted into the SQL text.
set -euo pipefail

psql -v ON_ERROR_STOP=1 \
  --username "$POSTGRES_USER" \
  --dbname "$POSTGRES_DB" \
  -v migrator_password="$SPOR_AKADEMI_MIGRATOR_PASSWORD" \
  -v app_password="$SPOR_AKADEMI_APP_PASSWORD" <<'SQL'
CREATE ROLE spor_akademi_migrator LOGIN PASSWORD :'migrator_password';
CREATE ROLE spor_akademi_app LOGIN PASSWORD :'app_password';

-- The migrator owns the database, so it can create module schemas (DEC-038) and, because the public
-- schema is owned by pg_database_owner, objects in public as well.
ALTER DATABASE spor_akademi OWNER TO spor_akademi_migrator;

-- Only the application role may connect besides the owner.
REVOKE ALL ON DATABASE spor_akademi FROM PUBLIC;
GRANT CONNECT ON DATABASE spor_akademi TO spor_akademi_app;

-- Privileges for objects the migrator creates later. They apply only to objects created by
-- spor_akademi_migrator; migrations must therefore never run as postgres.
ALTER DEFAULT PRIVILEGES FOR ROLE spor_akademi_migrator
  GRANT USAGE ON SCHEMAS TO spor_akademi_app;
ALTER DEFAULT PRIVILEGES FOR ROLE spor_akademi_migrator
  GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO spor_akademi_app;
ALTER DEFAULT PRIVILEGES FOR ROLE spor_akademi_migrator
  GRANT USAGE, SELECT ON SEQUENCES TO spor_akademi_app;
SQL
