using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SporAkademi.Api;
using SporAkademi.Infrastructure;
using SporAkademi.Infrastructure.Persistence;

PlatformRequirements.EnsureAvailable();

var builder = WebApplication.CreateBuilder(args);

// Locally set by scripts/run-api.sh; on the servers by the deployment environment (P0-30, P0-31).
var connectionString = builder.Configuration.GetConnectionString("App");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:App is not set. Run the API locally through scripts/run-api.sh.");
}

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddHealthChecks()
    .AddDbContextCheck<SporAkademiDbContext>(tags: ["ready"]);

var app = builder.Build();

// Health endpoints sit outside /api/v1 (docs/api/overview.md, NFR-REL-07). The default response writer
// returns only the status text, so no error details, versions or connection details are exposed.
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

app.Run();
