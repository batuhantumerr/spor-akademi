using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SporAkademi.Infrastructure.Persistence;

namespace SporAkademi.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers the infrastructure services. The connection string is the application's own
    /// (spor_akademi_app), which cannot change the schema (NFR-SEC-10).
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<SporAkademiDbContext>(options => options.UseSporAkademiDatabase(connectionString));

        return services;
    }
}
