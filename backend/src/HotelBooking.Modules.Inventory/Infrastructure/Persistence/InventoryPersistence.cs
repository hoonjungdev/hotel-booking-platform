using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Modules.Inventory.Infrastructure.Persistence;

internal static class InventoryPersistence
{
    internal const string Schema = "inventory";
    internal const string MigrationsHistoryTable = "__ef_migrations_history";

    /// <summary>
    /// Configures Inventory persistence consistently for runtime, design-time tooling, and integration tests.
    /// </summary>
    /// <param name="optionsBuilder">The context options builder to configure.</param>
    /// <param name="connectionString">The shared PostgreSQL database connection string.</param>
    internal static void Configure(
        DbContextOptionsBuilder optionsBuilder,
        string connectionString)
    {
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            npgsqlOptions.MigrationsHistoryTable(
                MigrationsHistoryTable,
                Schema));
    }
}
