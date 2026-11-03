using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SporAkademi.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class RevokeAppAccessToMigrationsHistory : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // The default privileges give the application role access to every table the migrator creates,
        // including the migrations history table. The application never runs migrations (ADR-0011), and
        // write access would let it mark migrations as applied or make them run again.
        migrationBuilder.Sql("REVOKE ALL ON TABLE public.__ef_migrations_history FROM spor_akademi_app;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.__ef_migrations_history TO spor_akademi_app;");
    }
}
