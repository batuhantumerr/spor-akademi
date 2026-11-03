using Microsoft.EntityFrameworkCore;

namespace SporAkademi.Infrastructure.Persistence;

/// <summary>
/// The single DbContext of the application (DEC-038). Each module maps its tables to its own schema.
/// The model is empty until the first module adds its entities (P1-04).
/// </summary>
public sealed class SporAkademiDbContext(DbContextOptions<SporAkademiDbContext> options) : DbContext(options);
