using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;
using HotelBooking.Modules.Property.Infrastructure.Persistence;
using HotelBooking.SharedKernel.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace HotelBooking.IntegrationTests.Property.Hotels;

/// <summary>
/// Proves Hotel persistence behavior against migrated PostgreSQL rather than an in-memory substitute.
/// </summary>
public sealed class HotelPersistenceTests : IClassFixture<PropertyPostgreSqlFixture>, IAsyncLifetime
{
    private readonly PropertyPostgreSqlFixture _fixture;

    /// <summary>Initializes the test class with its migrated PostgreSQL fixture.</summary>
    /// <param name="fixture">The shared PostgreSQL fixture for this test class.</param>
    public HotelPersistenceTests(PropertyPostgreSqlFixture fixture)
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
    public async Task Production_migration_creates_the_Property_schema_and_module_history_table()
    {
        await using var connection = new NpgsqlConnection(_fixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(
            """
            SELECT
                to_regclass('property.hotels') IS NOT NULL,
                to_regclass('property.__ef_migrations_history') IS NOT NULL;
            """,
            connection);

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();
        Assert.True(await reader.ReadAsync());
        Assert.True(reader.GetBoolean(0));
        Assert.True(reader.GetBoolean(1));
    }

    [Fact]
    public async Task Draft_hotel_round_trip_preserves_required_values_and_absent_optional_values()
    {
        HotelId hotelId = HotelId.From(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        DateTimeOffset createdAt = new(2026, 7, 20, 1, 30, 0, TimeSpan.Zero);

        Hotel hotel = Hotel.CreateDraft(
            id: hotelId,
            name: "Seoul River Hotel",
            slug: $"seoul-river-{hotelId.Value:N}",
            starRating: null,
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("KRW"),
            defaultLanguage: "ko",
            createdAt: createdAt);

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext assertContext = _fixture.CreateDbContext();
        Hotel persistedHotel = await assertContext.Hotels.SingleAsync(candidate => candidate.Id == hotelId);

        Assert.Equal(hotelId, persistedHotel.Id);
        Assert.Equal("Seoul River Hotel", persistedHotel.Name);
        Assert.Equal(hotel.Slug, persistedHotel.Slug);
        Assert.Equal(HotelStatus.Draft, persistedHotel.Status);
        Assert.Null(persistedHotel.StarRating);
        Assert.Equal("Asia/Seoul", persistedHotel.TimeZoneId);
        Assert.Equal(Currency.FromCode("KRW"), persistedHotel.SellingCurrency);
        Assert.Equal("ko", persistedHotel.DefaultLanguage);
        Assert.Equal(createdAt, persistedHotel.CreatedAt);
        Assert.Null(persistedHotel.UpdatedAt);
        Assert.Null(persistedHotel.Address);
        Assert.Null(persistedHotel.GeoLocation);
        Assert.Null(persistedHotel.ContactInfo);
        Assert.Null(persistedHotel.CheckInPolicy);
        Assert.Null(persistedHotel.HotelPolicy);
        Assert.Empty(persistedHotel.DomainEvents);
    }

    [Fact]
    public async Task Active_hotel_round_trip_preserves_complex_values_and_lifecycle_state()
    {
        HotelId hotelId = HotelId.From(Guid.Parse("10000000-0000-0000-0000-000000000002"));
        DateTimeOffset createdAt = new(2026, 7, 20, 2, 0, 0, TimeSpan.Zero);
        DateTimeOffset updatedAt = new(2026, 7, 20, 3, 0, 0, TimeSpan.Zero);

        Hotel hotel = Hotel.CreateDraft(
            id: hotelId,
            name: "Busan Harbor Hotel",
            slug: $"busan-harbor-{hotelId.Value:N}",
            starRating: StarRating.Create(5),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("KRW"),
            defaultLanguage: "en",
            createdAt: createdAt);

        var address = new Address(
            CountryCode: "KR",
            Region: "Busan",
            City: "Busan",
            StreetAddress: "1 Harbor Road",
            DetailAddress: "Ocean Tower",
            PostalCode: "48900");
        var geoLocation = new GeoLocation(35.1028m, 129.0403m);
        var contactInfo = new ContactInfo(
            Phone: "+82-51-000-0000",
            Email: "stay@busan-harbor.example",
            Website: "https://busan-harbor.example");
        var checkInPolicy = new CheckInPolicy(
            CheckInFrom: new TimeOnly(15, 0),
            CheckInUntil: new TimeOnly(23, 0),
            CheckOutUntil: new TimeOnly(11, 0),
            AllowsEarlyCheckIn: true,
            AllowsLateCheckOut: false);
        var hotelPolicy = new HotelPolicy(
            AllowsSmoking: false,
            AllowsPets: false,
            AllowsChildren: true,
            MinimumCheckInAge: 18,
            RequiresDeposit: true);

        hotel.UpdateLocation(address, geoLocation, updatedAt);
        hotel.UpdateContactInfo(contactInfo, updatedAt);
        hotel.UpdateCheckInPolicy(checkInPolicy, updatedAt);
        hotel.UpdateHotelPolicy(hotelPolicy, updatedAt);
        hotel.SubmitForReview();
        hotel.Publish(updatedAt);

        await using (PropertyDbContext arrangeContext = _fixture.CreateDbContext())
        {
            arrangeContext.Hotels.Add(hotel);
            await arrangeContext.SaveChangesAsync();
        }

        await using PropertyDbContext assertContext = _fixture.CreateDbContext();
        Hotel persistedHotel = await assertContext.Hotels.SingleAsync(candidate => candidate.Id == hotelId);

        Assert.Equal(HotelStatus.Active, persistedHotel.Status);
        Assert.Equal(StarRating.Create(5), persistedHotel.StarRating);
        Assert.Equal(address, persistedHotel.Address);
        Assert.Equal(geoLocation, persistedHotel.GeoLocation);
        Assert.Equal(contactInfo, persistedHotel.ContactInfo);
        Assert.Equal(checkInPolicy, persistedHotel.CheckInPolicy);
        Assert.Equal(hotelPolicy, persistedHotel.HotelPolicy);
        Assert.Equal(updatedAt, persistedHotel.UpdatedAt);
        Assert.Empty(persistedHotel.DomainEvents);
    }

    [Fact]
    public async Task PostgreSQL_rejects_duplicate_hotel_slugs()
    {
        const string duplicateSlug = "duplicate-hotel";

        Hotel firstHotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("10000000-0000-0000-0000-000000000003")),
            duplicateSlug,
            "First Hotel");
        Hotel secondHotel = CreateDraftHotel(
            HotelId.From(Guid.Parse("10000000-0000-0000-0000-000000000004")),
            duplicateSlug,
            "Second Hotel");

        await using PropertyDbContext context = _fixture.CreateDbContext();
        context.Hotels.AddRange(firstHotel, secondHotel);

        DbUpdateException exception = await Assert.ThrowsAsync<DbUpdateException>(
            () => context.SaveChangesAsync());

        var postgresException = Assert.IsType<PostgresException>(exception.InnerException);
        Assert.Equal(PostgresErrorCodes.UniqueViolation, postgresException.SqlState);
        Assert.Equal("ux_hotels_slug", postgresException.ConstraintName);
    }

    private static Hotel CreateDraftHotel(HotelId hotelId, string slug, string name)
    {
        return Hotel.CreateDraft(
            id: hotelId,
            name: name,
            slug: slug,
            starRating: StarRating.Create(4),
            timeZoneId: "Asia/Seoul",
            sellingCurrency: Currency.FromCode("KRW"),
            defaultLanguage: "ko",
            createdAt: new DateTimeOffset(2026, 7, 20, 4, 0, 0, TimeSpan.Zero));
    }
}
