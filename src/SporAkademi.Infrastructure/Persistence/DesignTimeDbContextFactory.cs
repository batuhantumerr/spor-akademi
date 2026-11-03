using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SporAkademi.Infrastructure.Persistence;

/// <summary>
/// Used only by the dotnet-ef tool. Connects as spor_akademi_migrator (ADR-0011); scripts/ef.sh builds the
/// connection string from .env and passes it in the environment, so it is never stored in a file.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<SporAkademiDbContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__Migrator";

    public SporAkademiDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"{ConnectionStringVariable} is not set. Run the EF Core tools through scripts/ef.sh.");
        }

        var builder = new DbContextOptionsBuilder<SporAkademiDbContext>();
        builder.UseSporAkademiDatabase(connectionString);

        return new SporAkademiDbContext(builder.Options);
    }
}
