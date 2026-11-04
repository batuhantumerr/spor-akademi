# Admin app image (P0-29, DEC-081). Build from the repository root:
#   docker build -f docker/admin/Dockerfile -t spor-akademi-admin .
#
# Base images are pinned by tag and digest; Dependabot updates both (docker ecosystem).

# Node version matches web/admin/.nvmrc (DEC-075); .npmrc's engine-strict rejects any other version.
FROM node:26.13.0-trixie-slim@sha256:e4db4c914bf8ad241bd24804fc25d272d89bad97e68cdc8a1414862790a02c39 AS build
WORKDIR /app

# Install first, from the package files only, so this layer is reused until a dependency changes.
COPY web/admin/package.json web/admin/package-lock.json web/admin/.npmrc ./
RUN npm ci

COPY web/admin/ ./
RUN npm run build

# Static files only. The unprivileged nginx image runs as a non-root user and listens on 8080. Requests to
# /api are routed to the API by the reverse proxy (P0-30, DEC-054), not by this container.
# The -slim variant leaves out packages a static file server does not use (curl, and the font libraries
# that pull in brotli and expat), so their vulnerabilities are not in the image at all.
FROM nginxinc/nginx-unprivileged:1.30.6-alpine-slim@sha256:5d8bb9437d74a600ec5dbf90c035451c724dc33eac8d3a71078f6f8612c345f6
COPY docker/admin/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/admin/browser /usr/share/nginx/html
EXPOSE 8080
