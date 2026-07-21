using HotelBooking.Modules.Inventory.Domain.InventoryDates;
using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HotelBooking.IntegrationTests.Inventory.InventoryDates;

/// <summary>
/// Proves Inventory Date persistence and relational constraints against migrated PostgreSQL.
/// </summary>
public sealed class InventoryDatePersistenceTests : IClassFixture<InventoryPostgreSqlFixture>, IAsyncLifetime
{
    private static readonly HotelId SeoulHotelId =
        HotelId.From(Guid.Parse("40000000-0000-0000-0000-000000000001"));
    private static readonly RoomTypeId DeluxeRoomTypeId =
        RoomTypeId.From(Guid.Parse("50000000-0000-0000-0000-000000000001"));

    private readonly InventoryPostgreSqlFixture _fixture;

    /// <summary>Initializes the test class with its migrated PostgreSQL fixture.</summary>
    /// <param name="fixture">The shared Inventory PostgreSQL fixture.</param>
    public InventoryDatePersistenceTests(InventoryPostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    /// <inheritdoc />
    public Task InitializeAsync()
    {
        return _fixture.ResetAsync();
    }

    /// <inheritdoc />
    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Production_migration_creates_Inventory_schema_table_history_and_constraints()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT
                to_regclass('inventory.inventory_dates') IS NOT NULL,
                to_regclass('inventory.__ef_migrations_history') IS NOT NULL,
                to_regclass('inventory.ux_inventory_dates_hotel_id_room_type_id_occupied_date') IS NOT NULL,
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'ck_inventory_dates_quantities_non_negative'),
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'ck_inventory_dates_committed_quantity_within_total');
            """,
            connection);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetBoolean(0));
        Assert.True(reader.GetBoolean(1));
        Assert.True(reader.GetBoolean(2));
        Assert.True(reader.GetBoolean(3));
        Assert.True(reader.GetBoolean(4));
    }

    [Fact]
    public async Task Initial_inventory_date_round_trip_preserves_values_and_available_quantity()
    {
        InventoryDateId inventoryDateId =
            InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000001"));
        DateOnly occupiedDate = new(2026, 8, 1);
        InventoryDate inventoryDate = InventoryDate.Create(
            inventoryDateId,
            SeoulHotelId,
            DeluxeRoomTypeId,
            occupiedDate,
            totalQuantity: 10);

        await using (InventoryDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.InventoryDates.Add(inventoryDate);
            await arrangeContext.SaveChangesAsync();
        }

        await using InventoryDbContext assertContext = _fixture.CreateDbContext();
        InventoryDate persistedInventoryDate = await assertContext.InventoryDates
            .SingleAsync(candidate => candidate.Id == inventoryDateId);

        Assert.Equal(inventoryDateId, persistedInventoryDate.Id);
        Assert.Equal(SeoulHotelId, persistedInventoryDate.HotelId);
        Assert.Equal(DeluxeRoomTypeId, persistedInventoryDate.RoomTypeId);
        Assert.Equal(occupiedDate, persistedInventoryDate.OccupiedDate);
        Assert.Equal(10, persistedInventoryDate.TotalQuantity);
        Assert.Equal(0, persistedInventoryDate.HeldQuantity);
        Assert.Equal(0, persistedInventoryDate.BookedQuantity);
        Assert.Equal(0, persistedInventoryDate.ClosedQuantity);
        Assert.Equal(10, persistedInventoryDate.AvailableQuantity);
        Assert.Empty(persistedInventoryDate.DomainEvents);
    }

    [Fact]
    public async Task Changed_inventory_quantities_round_trip_preserves_each_commitment()
    {
        InventoryDate inventoryDate = CreateInventoryDate(
            InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000002")),
            SeoulHotelId,
            DeluxeRoomTypeId,
            new DateOnly(2026, 8, 2),
            totalQuantity: 10);
        inventoryDate.IncreaseHeldQuantity(4);
        inventoryDate.ConvertHeldToBookedQuantity(2);
        inventoryDate.IncreaseClosedQuantity(3);

        await using (InventoryDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.InventoryDates.Add(inventoryDate);
            await arrangeContext.SaveChangesAsync();
        }

        await using InventoryDbContext assertContext = _fixture.CreateDbContext();
        InventoryDate persistedInventoryDate = await assertContext.InventoryDates.SingleAsync();

        Assert.Equal(10, persistedInventoryDate.TotalQuantity);
        Assert.Equal(2, persistedInventoryDate.HeldQuantity);
        Assert.Equal(2, persistedInventoryDate.BookedQuantity);
        Assert.Equal(3, persistedInventoryDate.ClosedQuantity);
        Assert.Equal(3, persistedInventoryDate.AvailableQuantity);
    }

    [Fact]
    public async Task PostgreSQL_rejects_duplicate_hotel_room_type_and_occupied_date()
    {
        InventoryDate firstInventoryDate = CreateInventoryDate(
            InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000003")),
            SeoulHotelId,
            DeluxeRoomTypeId,
            new DateOnly(2026, 8, 3),
            totalQuantity: 5);
        InventoryDate duplicateInventoryDate = CreateInventoryDate(
            InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000004")),
            SeoulHotelId,
            DeluxeRoomTypeId,
            new DateOnly(2026, 8, 3),
            totalQuantity: 7);

        await using InventoryDbContext context = _fixture.CreateDbContext();
        context.InventoryDates.AddRange(firstInventoryDate, duplicateInventoryDate);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal(
            "ux_inventory_dates_hotel_id_room_type_id_occupied_date",
            postgresException.ConstraintName);
    }

    [Fact]
    public async Task PostgreSQL_allows_inventory_dates_that_differ_by_hotel_room_type_or_occupied_date()
    {
        HotelId anotherHotelId =
            HotelId.From(Guid.Parse("40000000-0000-0000-0000-000000000002"));
        RoomTypeId anotherRoomTypeId =
            RoomTypeId.From(Guid.Parse("50000000-0000-0000-0000-000000000002"));
        DateOnly occupiedDate = new(2026, 8, 4);

        InventoryDate[] inventoryDates =
        [
            CreateInventoryDate(
                InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000005")),
                SeoulHotelId,
                DeluxeRoomTypeId,
                occupiedDate,
                totalQuantity: 5),
            CreateInventoryDate(
                InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000006")),
                anotherHotelId,
                DeluxeRoomTypeId,
                occupiedDate,
                totalQuantity: 5),
            CreateInventoryDate(
                InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000007")),
                SeoulHotelId,
                anotherRoomTypeId,
                occupiedDate,
                totalQuantity: 5),
            CreateInventoryDate(
                InventoryDateId.From(Guid.Parse("60000000-0000-0000-0000-000000000008")),
                SeoulHotelId,
                DeluxeRoomTypeId,
                occupiedDate.AddDays(1),
                totalQuantity: 5)
        ];

        await using InventoryDbContext context = _fixture.CreateDbContext();
        context.InventoryDates.AddRange(inventoryDates);
        await context.SaveChangesAsync();

        Assert.Equal(4, await context.InventoryDates.CountAsync());
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public async Task PostgreSQL_rejects_negative_committed_inventory_quantity(
        int heldQuantity,
        int bookedQuantity,
        int closedQuantity)
    {
        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(() =>
            InsertInventoryDateAsync(
                totalQuantity: 1,
                heldQuantity,
                bookedQuantity,
                closedQuantity));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal("ck_inventory_dates_quantities_non_negative", exception.ConstraintName);
    }

    [Fact]
    public async Task PostgreSQL_rejects_negative_total_quantity()
    {
        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(() =>
            InsertInventoryDateAsync(
                totalQuantity: -1,
                heldQuantity: 0,
                bookedQuantity: 0,
                closedQuantity: 0));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.True(
            exception.ConstraintName is
                "ck_inventory_dates_quantities_non_negative" or
                "ck_inventory_dates_committed_quantity_within_total");
    }

    [Fact]
    public async Task PostgreSQL_rejects_committed_quantity_that_exceeds_total_quantity()
    {
        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(() =>
            InsertInventoryDateAsync(
                totalQuantity: 5,
                heldQuantity: 2,
                bookedQuantity: 2,
                closedQuantity: 2));

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal(
            "ck_inventory_dates_committed_quantity_within_total",
            exception.ConstraintName);
    }

    private static InventoryDate CreateInventoryDate(
        InventoryDateId inventoryDateId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        DateOnly occupiedDate,
        int totalQuantity)
    {
        return InventoryDate.Create(
            inventoryDateId,
            hotelId,
            roomTypeId,
            occupiedDate,
            totalQuantity);
    }

    private async Task InsertInventoryDateAsync(
        int totalQuantity,
        int heldQuantity,
        int bookedQuantity,
        int closedQuantity)
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            INSERT INTO inventory.inventory_dates (
                id,
                hotel_id,
                room_type_id,
                occupied_date,
                total_quantity,
                held_quantity,
                booked_quantity,
                closed_quantity)
            VALUES (
                @id,
                @hotelId,
                @roomTypeId,
                @occupiedDate,
                @totalQuantity,
                @heldQuantity,
                @bookedQuantity,
                @closedQuantity);
            """,
            connection);
        command.Parameters.AddWithValue(
            "id",
            Guid.Parse("60000000-0000-0000-0000-000000000099"));
        command.Parameters.AddWithValue("hotelId", SeoulHotelId.Value);
        command.Parameters.AddWithValue("roomTypeId", DeluxeRoomTypeId.Value);
        command.Parameters.AddWithValue("occupiedDate", new DateOnly(2026, 8, 10));
        command.Parameters.AddWithValue("totalQuantity", totalQuantity);
        command.Parameters.AddWithValue("heldQuantity", heldQuantity);
        command.Parameters.AddWithValue("bookedQuantity", bookedQuantity);
        command.Parameters.AddWithValue("closedQuantity", closedQuantity);

        await command.ExecuteNonQueryAsync();
    }
}
