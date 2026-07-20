using HotelBooking.Modules.Property.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace HotelBooking.IntegrationTests.Property.Hotels;

/// <summary>
/// Provides an isolated PostgreSQL 17 container with production Property migrations applied.
/// </summary>
public sealed class PropertyPostgreSqlFixture : IAsyncLifetime
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

        await using PropertyDbContext context = CreateDbContext();
        await context.Database.MigrateAsync();
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    /// <summary>Creates a Property context using the same provider configuration as production.</summary>
    /// <returns>A new Property context connected to the fixture database.</returns>
    internal PropertyDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<PropertyDbContext>();

        PropertyPersistence.Configure(optionsBuilder, ConnectionString);

        return new PropertyDbContext(optionsBuilder.Options);
    }

    /// <summary>Removes persisted Property module rows so each test starts from an isolated database state.</summary>
    internal async Task ResetAsync()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            TRUNCATE TABLE
                property.room_type_bed_compositions,
                property.room_types,
                property.hotels
            RESTART IDENTITY;
            """,
            connection);

        await command.ExecuteNonQueryAsync();
    }
}
