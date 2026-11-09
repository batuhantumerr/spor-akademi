# Migrator image (P0-31, DEC-089, ADR-0011): the EF Core migration bundle and the idempotent SQL script,
# both produced from one build. Build from the repository root:
#   docker build -f docker/migrator/Dockerfile -t spor-akademi-migrator .
# Run as a one-off job with the migrator connection string in the environment, never on the command line:
#   docker run --rm --env ConnectionStrings__Migrator spor-akademi-migrator
#
# Base images are pinned by tag and digest; Dependabot updates both (docker ecosystem).

FROM mcr.microsoft.com/dotnet/sdk:10.0.403-noble@sha256:de4fe90a98845d75910b9769662465c84de1a934665c431279d1382b2bddc02b AS build
WORKDIR /src

# Restore first, from the project and build files only, so this layer is reused until a dependency changes.
# Locked mode: the image gets exactly the packages in the lock files, like CI (P0-38, DEC-080). dotnet-ef
# comes from the tool manifest, whose version CI checks against EF Core (scripts/check-ef-tool-version.sh).
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY .config/dotnet-tools.json .config/
COPY src/SporAkademi.Domain/SporAkademi.Domain.csproj src/SporAkademi.Domain/packages.lock.json src/SporAkademi.Domain/
COPY src/SporAkademi.Application/SporAkademi.Application.csproj src/SporAkademi.Application/packages.lock.json src/SporAkademi.Application/
COPY src/SporAkademi.Infrastructure/SporAkademi.Infrastructure.csproj src/SporAkademi.Infrastructure/packages.lock.json src/SporAkademi.Infrastructure/
COPY src/SporAkademi.Api/SporAkademi.Api.csproj src/SporAkademi.Api/packages.lock.json src/SporAkademi.Api/
RUN dotnet tool restore && dotnet restore --locked-mode src/SporAkademi.Api/SporAkademi.Api.csproj

COPY src/ src/

# One linux-x64 build, then both outputs from it with --no-build, so the reviewed SQL and the bundle cannot
# come from different builds (P0-31; in P0-27 a script made from an older build missed a migration).
# - RestoreLockedMode: dotnet-ef builds the bundle in a project of its own, which restores again; this makes
#   that restore locked as well, instead of quietly rewriting the lock files (DEC-093).
# - DesignTimeDbContextFactory needs a connection string even though nothing connects here; this one has no
#   password and no reachable server.
RUN export RestoreLockedMode=true \
 && export ConnectionStrings__Migrator="Host=localhost;Database=spor_akademi;Username=spor_akademi_migrator" \
 && dotnet build src/SporAkademi.Api/SporAkademi.Api.csproj --configuration Release --runtime linux-x64 --no-restore \
 && dotnet ef migrations bundle --no-build --configuration Release \
      --self-contained --runtime linux-x64 \
      --project src/SporAkademi.Infrastructure --startup-project src/SporAkademi.Api \
      --output /out/efbundle \
 && dotnet ef migrations script --idempotent --no-build --configuration Release --runtime linux-x64 \
      --project src/SporAkademi.Infrastructure --startup-project src/SporAkademi.Api \
      --output /out/migrations.sql

# The bundle is self-contained, so the runtime-deps image is enough: no .NET runtime or SDK, no shell, no
# package manager, and a non-root user. Globalization runs in invariant mode (set by the image); migrations
# do not format culture-specific text.
FROM mcr.microsoft.com/dotnet/runtime-deps:10.0.14-noble-chiseled@sha256:e9346023014b406b9bc8be969b3aa4d39efa3a43b6abcc495f01be566acb10e9
WORKDIR /migrations
COPY --from=build /out/efbundle /out/migrations.sql ./
USER app
ENTRYPOINT ["./efbundle"]
