# API image (P0-29, DEC-081). Build from the repository root:
#   docker build -f docker/api/Dockerfile -t spor-akademi-api .
#
# Base images are pinned by tag and digest; Dependabot updates both (docker ecosystem).

FROM mcr.microsoft.com/dotnet/sdk:10.0.404-noble@sha256:fe0fd4e93bd2fbb47c883b7a6369e5c8d3db8bf01cdc5981f6ee35fbdf3ff4fb AS build
WORKDIR /src

# Restore first, from the project and build files only, so this layer is reused until a dependency changes.
# Locked mode: the image gets exactly the packages in the lock files, like CI (P0-38, DEC-080).
COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/SporAkademi.Domain/SporAkademi.Domain.csproj src/SporAkademi.Domain/packages.lock.json src/SporAkademi.Domain/
COPY src/SporAkademi.Application/SporAkademi.Application.csproj src/SporAkademi.Application/packages.lock.json src/SporAkademi.Application/
COPY src/SporAkademi.Infrastructure/SporAkademi.Infrastructure.csproj src/SporAkademi.Infrastructure/packages.lock.json src/SporAkademi.Infrastructure/
COPY src/SporAkademi.Api/SporAkademi.Api.csproj src/SporAkademi.Api/packages.lock.json src/SporAkademi.Api/
RUN dotnet restore --locked-mode src/SporAkademi.Api/SporAkademi.Api.csproj

COPY src/ src/
RUN dotnet publish src/SporAkademi.Api/SporAkademi.Api.csproj \
    --configuration Release --no-restore --output /app -p:UseAppHost=false

# Chiseled: non-root user, no shell, no package manager. The -extra variant adds ICU and tzdata, which the
# API needs for school time zones and tr-TR formats (PlatformRequirements fails at startup without them).
FROM mcr.microsoft.com/dotnet/aspnet:10.0.15-noble-chiseled-extra@sha256:cae5b962bfb786606def03d250c6a645cc98839095c89e806e4bd8978a68115b
WORKDIR /app
COPY --from=build /app .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "SporAkademi.Api.dll"]
