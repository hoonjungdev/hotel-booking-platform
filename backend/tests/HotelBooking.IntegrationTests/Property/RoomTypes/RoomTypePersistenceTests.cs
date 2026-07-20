using HotelBooking.IntegrationTests.Property.Hotels;
using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes;
using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.Modules.Property.Infrastructure.Persistence;
using HotelBooking.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HotelBooking.IntegrationTests.Property.RoomTypes;

/// <summary>
/// Proves Room Type persistence and relational constraints against migrated PostgreSQL.
/// </summary>
public sealed class RoomTypePersistenceTests : IClassFixture<PropertyPostgreSqlFixture>, IAsyncLifetime
{
    private readonly PropertyPostgreSqlFixture _fixture;

    /// <summary>Initializes the test class with its migrated PostgreSQL fixture.</summary>
    /// <param name="fixture">The shared Property PostgreSQL fixture.</param>
    public RoomTypePersistenceTests(PropertyPostgreSqlFixture fixture)
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
    public async Task Production_migration_creates_Room_Type_tables_and_constraints()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT
                to_regclass('property.room_types') IS NOT NULL,
                to_regclass('property.room_type_bed_compositions') IS NOT NULL,
                to_regclass('property.ux_room_types_hotel_id_code') IS NOT NULL,
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'fk_room_types_hotels_hotel_id'),
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'fk_room_type_bed_compositions_room_types_room_type_id'),
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'ck_room_types_occupancy_valid'),
                EXISTS (
                    SELECT 1
                    FROM pg_constraint
                    WHERE conname = 'ck_room_type_bed_compositions_quantity_positive');
            """,
            connection);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetBoolean(0));
        Assert.True(reader.GetBoolean(1));
        Assert.True(reader.GetBoolean(2));
        Assert.True(reader.GetBoolean(3));
        Assert.True(reader.GetBoolean(4));
        Assert.True(reader.GetBoolean(5));
        Assert.True(reader.GetBoolean(6));
    }

    [Fact]
    public async Task Draft_room_type_round_trip_preserves_values_and_bed_compositions()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000001")),
            "seoul-central");
        RoomTypeId roomTypeId = RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));
        DateTimeOffset createdAt = new(2026, 7, 20, 5, 0, 0, TimeSpan.Zero);
        RoomType roomType = RoomType.CreateDraft(
            id: roomTypeId,
            hotelId: hotel.Id,
            name: "Deluxe Family",
            code: RoomTypeCode.Create("dlx-family"),
            occupancy: Occupancy.Create(maxAdults: 3, maxChildren: 2, maxOccupancy: 4),
            bedCompositions:
            [
                BedComposition.Create(BedType.King, 1),
                BedComposition.Create(BedType.Single, 2)
            ],
            createdAt: createdAt);

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            arrangeContext.RoomTypes.Add(roomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext assertContext = _fixture.CreateDbContext();
        RoomType persistedRoomType = await assertContext.RoomTypes
            .SingleAsync(candidate => candidate.Id == roomTypeId);

        Assert.Equal(roomTypeId, persistedRoomType.Id);
        Assert.Equal(hotel.Id, persistedRoomType.HotelId);
        Assert.Equal("Deluxe Family", persistedRoomType.Name);
        Assert.Equal(RoomTypeCode.Create("DLX-FAMILY"), persistedRoomType.Code);
        Assert.Equal(RoomTypeStatus.Draft, persistedRoomType.Status);
        Assert.Equal(Occupancy.Create(3, 2, 4), persistedRoomType.Occupancy);
        Assert.Equal(createdAt, persistedRoomType.CreatedAt);
        Assert.Null(persistedRoomType.UpdatedAt);
        Assert.Contains(
            BedComposition.Create(BedType.King, 1),
            persistedRoomType.BedCompositions);
        Assert.Contains(
            BedComposition.Create(BedType.Single, 2),
            persistedRoomType.BedCompositions);
        Assert.Equal(2, persistedRoomType.BedCompositions.Count);
        Assert.Empty(persistedRoomType.DomainEvents);
    }

    [Fact]
    public async Task Suspended_room_type_round_trip_preserves_lifecycle_state()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000002")),
            "busan-harbor");
        RoomType roomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000002")),
            hotel.Id,
            "Deluxe Double",
            "DLX");
        roomType.Activate();
        roomType.Suspend();

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            arrangeContext.RoomTypes.Add(roomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext assertContext = _fixture.CreateDbContext();
        RoomType persistedRoomType = await assertContext.RoomTypes.SingleAsync();

        Assert.Equal(RoomTypeStatus.Suspended, persistedRoomType.Status);
        Assert.False(persistedRoomType.IsSellable);
    }

    [Fact]
    public async Task PostgreSQL_rejects_room_type_for_unknown_hotel()
    {
        RoomType roomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000003")),
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000099")),
            "Standard Twin",
            "TWN");

        await using PropertyDbContext context = _fixture.CreateDbContext();
        context.RoomTypes.Add(roomType);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgresException.SqlState);
        Assert.Equal("fk_room_types_hotels_hotel_id", postgresException.ConstraintName);
    }

    [Fact]
    public async Task PostgreSQL_rejects_duplicate_room_type_code_within_same_hotel()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000003")),
            "jeju-coast");
        RoomType firstRoomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000004")),
            hotel.Id,
            "First Standard",
            "std");
        RoomType secondRoomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000005")),
            hotel.Id,
            "Second Standard",
            "STD");

        await using PropertyDbContext context = _fixture.CreateDbContext();
        context.Hotels.Add(hotel);
        context.RoomTypes.AddRange(firstRoomType, secondRoomType);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal("ux_room_types_hotel_id_code", postgresException.ConstraintName);
    }

    [Fact]
    public async Task PostgreSQL_allows_same_room_type_code_for_different_hotels()
    {
        Hotel firstHotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000004")),
            "first-city-hotel");
        Hotel secondHotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000005")),
            "second-city-hotel");
        RoomType firstRoomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000006")),
            firstHotel.Id,
            "Standard",
            "STD");
        RoomType secondRoomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000007")),
            secondHotel.Id,
            "Standard",
            "STD");

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.AddRange(firstHotel, secondHotel);
            arrangeContext.RoomTypes.AddRange(firstRoomType, secondRoomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext assertContext = _fixture.CreateDbContext();
        Assert.Equal(2, await assertContext.RoomTypes.CountAsync());
    }

    [Theory]
    [InlineData(0, 0, 1)]
    [InlineData(1, -1, 1)]
    [InlineData(1, 0, 0)]
    [InlineData(2, 0, 1)]
    [InlineData(1, 2, 1)]
    [InlineData(1, 1, 3)]
    public async Task PostgreSQL_rejects_invalid_room_type_occupancy(
        int maxAdults,
        int maxChildren,
        int maxOccupancy)
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000009")),
            "invalid-occupancy-hotel");

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            await arrangeContext.SaveChangesAsync();
        }

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO property.room_types (
                id,
                hotel_id,
                name,
                code,
                status,
                created_at,
                max_adults,
                max_children,
                max_occupancy)
            VALUES (
                @id,
                @hotelId,
                'Invalid Occupancy',
                @code,
                'Draft',
                @createdAt,
                @maxAdults,
                @maxChildren,
                @maxOccupancy);
            """,
            connection);
        command.Parameters.AddWithValue(
            "id",
            Guid.Parse("30000000-0000-0000-0000-000000000011"));
        command.Parameters.AddWithValue("hotelId", hotel.Id.Value);
        command.Parameters.AddWithValue("code", "INVALID");
        command.Parameters.AddWithValue(
            "createdAt",
            new DateTimeOffset(2026, 7, 20, 6, 0, 0, TimeSpan.Zero));
        command.Parameters.AddWithValue("maxAdults", maxAdults);
        command.Parameters.AddWithValue("maxChildren", maxChildren);
        command.Parameters.AddWithValue("maxOccupancy", maxOccupancy);

        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(
            () => command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal("ck_room_types_occupancy_valid", exception.ConstraintName);
    }

    [Fact]
    public async Task PostgreSQL_rejects_non_positive_bed_composition_quantity()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000008")),
            "invalid-bed-quantity-hotel");
        RoomType roomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000010")),
            hotel.Id,
            "Valid Room",
            "VALID");

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            arrangeContext.RoomTypes.Add(roomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            """
            INSERT INTO property.room_type_bed_compositions (
                bed_type,
                quantity,
                room_type_id)
            VALUES ('Single', 0, @roomTypeId);
            """,
            connection);
        command.Parameters.AddWithValue("roomTypeId", roomType.Id.Value);

        PostgresException exception = await Assert.ThrowsAsync<PostgresException>(
            () => command.ExecuteNonQueryAsync());

        Assert.Equal(PostgresErrorCodes.CheckViolation, exception.SqlState);
        Assert.Equal(
            "ck_room_type_bed_compositions_quantity_positive",
            exception.ConstraintName);
    }

    [Fact]
    public async Task Deleting_room_type_cascades_to_owned_bed_compositions()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000006")),
            "cascade-hotel");
        RoomType roomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000008")),
            hotel.Id,
            "Cascade Room",
            "CASCADE");

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            arrangeContext.RoomTypes.Add(roomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();
        await using (var deleteCommand = new NpgsqlCommand(
            "DELETE FROM property.room_types WHERE id = @roomTypeId;",
            connection))
        {
            deleteCommand.Parameters.AddWithValue("roomTypeId", roomType.Id.Value);
            Assert.Equal(1, await deleteCommand.ExecuteNonQueryAsync());
        }

        await using var command = new NpgsqlCommand(
            "SELECT COUNT(*) FROM property.room_type_bed_compositions;",
            connection);

        Assert.Equal(0L, (long)(await command.ExecuteScalarAsync())!);
    }

    [Fact]
    public async Task PostgreSQL_restricts_deleting_hotel_that_still_has_room_type()
    {
        Hotel hotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000007")),
            "restricted-hotel");
        RoomType roomType = CreateDraftRoomType(
            RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000009")),
            hotel.Id,
            "Retained Room",
            "RETAINED");

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            arrangeContext.RoomTypes.Add(roomType);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext actContext = _fixture.CreateDbContext();
        Hotel persistedHotel = await actContext.Hotels.SingleAsync();
        actContext.Hotels.Remove(persistedHotel);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => actContext.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgresException.SqlState);
        Assert.Equal("fk_room_types_hotels_hotel_id", postgresException.ConstraintName);
    }

    private static Hotel CreateDraftHotel(HotelId hotelId, string slug)
    {
        return Hotel.CreateDraft(
            id: hotelId,
            name: $"Hotel {hotelId.Value:N}",
            slug: slug,
            starRating: null,
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("KRW"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 20, 4, 0, 0, TimeSpan.Zero));
    }

    private static RoomType CreateDraftRoomType(
        RoomTypeId roomTypeId,
        HotelId hotelId,
        string name,
        string code)
    {
        return RoomType.CreateDraft(
            id: roomTypeId,
            hotelId: hotelId,
            name: name,
            code: RoomTypeCode.Create(code),
            occupancy: Occupancy.Create(2, 1, 3),
            bedCompositions: [BedComposition.Create(BedType.Double, 1)],
            createdAt: new DateTimeOffset(2026, 7, 20, 5, 0, 0, TimeSpan.Zero));
    }
}
