using HotelBooking.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HotelBooking.IntegrationTests.Inventory.InventoryDates;

/// <summary>
/// Provides an isolated PostgreSQL 17 container with production Inventory migrations applied.
/// </summary>
public sealed class InventoryPostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("hotelbooking")
        .WithUsername("hotelbooking")
        .WithPassword("hotelbooking")
        .Build();

    /// <summary>Gets the dynamically assigned Testcontainer connection string.</summary>
    internal string ConnectionString => _container.GetConnectionString();

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using InventoryDbContext context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>Creates an Inventory context using the same provider configuration as production.</summary>
    /// <returns>A new Inventory context connected to the fixture database.</returns>
    internal InventoryDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

        InventoryPersistence.Configure(optionsBuilder, ConnectionString);

        return new InventoryDbContext(optionsBuilder.Options);
    }

    /// <summary>Removes persisted Inventory module rows so each test starts from an isolated database state.</summary>
    internal async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            "TRUNCATE TABLE inventory.inventory_dates;",
            connection);

        await command.ExecuteNonQueryAsync();
    }
}
