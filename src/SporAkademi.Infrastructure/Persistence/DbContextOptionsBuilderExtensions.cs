using Microsoft.EntityFrameworkCore;

namespace SporAkademi.Infrastructure.Persistence;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Database settings shared by the application and the design-time tools, so migrations are generated
    /// from the same model the application uses.
    /// </summary>
    public static DbContextOptionsBuilder UseSporAkademiDatabase(
        this DbContextOptionsBuilder builder,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder
            .UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "public"))
            // Tables, columns, keys and indexes are named in snake_case (DEC-079).
            .UseSnakeCaseNamingConvention();
    }
}
