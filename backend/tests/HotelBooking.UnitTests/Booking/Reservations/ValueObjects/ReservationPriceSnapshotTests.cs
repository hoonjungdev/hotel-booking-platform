using HotelBooking.Modules.Booking.Domain.References;
using HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Booking.Reservations.ValueObjects;

/// <summary>Proves completeness, consistency, ordering, and total calculation for agreed Reservation prices.</summary>
public class ReservationPriceSnapshotTests
{
    private static readonly PriceQuoteId ExistingPriceQuoteId =
        PriceQuoteId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));
    private static readonly HotelId ExistingHotelId =
        HotelId.From(Guid.Parse("40000000-0000-0000-0000-000000000001"));
    private static readonly RoomTypeId ExistingRoomTypeId =
        RoomTypeId.From(Guid.Parse("50000000-0000-0000-0000-000000000001"));
    private static readonly RatePlanId ExistingRatePlanId =
        RatePlanId.From(Guid.Parse("60000000-0000-0000-0000-000000000001"));
    private static readonly Currency KoreanWon = Currency.FromCode("KRW");

    [Fact]
    public void Create_preserves_a_complete_ordered_offer_and_calculates_total_price()
    {
        StayDateRange stay = CreateTwoNightStay();
        RequestedOccupancy occupancy = RequestedOccupancy.Create(2, 1);
        ReservationNightlyPrice firstNight = CreateNightlyPrice(
            stay.CheckInDate,
            150_000m);
        ReservationNightlyPrice secondNight = CreateNightlyPrice(
            stay.CheckInDate.AddDays(1),
            180_000m);
        ReservationNightlyPrice[] suppliedPrices = [secondNight, firstNight];
        CancellationPolicySnapshot cancellationPolicy = CreateCancellationPolicy();

        ReservationPriceSnapshot snapshot = ReservationPriceSnapshot.Create(
            ExistingPriceQuoteId,
            ExistingHotelId,
            ExistingRoomTypeId,
            ExistingRatePlanId,
            stay,
            occupancy,
            suppliedPrices,
            cancellationPolicy);
        suppliedPrices[0] = firstNight;

        Assert.Equal(ExistingPriceQuoteId, snapshot.PriceQuoteId);
        Assert.Equal(ExistingHotelId, snapshot.HotelId);
        Assert.Equal(ExistingRoomTypeId, snapshot.RoomTypeId);
        Assert.Equal(ExistingRatePlanId, snapshot.RatePlanId);
        Assert.Equal(stay, snapshot.StayDateRange);
        Assert.Equal(occupancy, snapshot.RequestedOccupancy);
        Assert.Collection(
            snapshot.NightlyPrices,
            nightlyPrice =>
            {
                Assert.Equal(stay.CheckInDate, nightlyPrice.OccupiedDate);
                Assert.Equal(Money.Create(150_000m, KoreanWon), nightlyPrice.Price);
            },
            nightlyPrice =>
            {
                Assert.Equal(stay.CheckInDate.AddDays(1), nightlyPrice.OccupiedDate);
                Assert.Equal(Money.Create(180_000m, KoreanWon), nightlyPrice.Price);
            });
        Assert.Equal(Money.Create(330_000m, KoreanWon), snapshot.TotalPrice);
        Assert.Same(cancellationPolicy, snapshot.CancellationPolicy);
    }

    [Fact]
    public void Create_allows_a_free_night()
    {
        StayDateRange stay = new(
            new DateOnly(2026, 8, 15),
            new DateOnly(2026, 8, 16));

        ReservationPriceSnapshot snapshot = CreateSnapshot(
            stay,
            [CreateNightlyPrice(stay.CheckInDate, 0m)]);

        Assert.Equal(Money.Zero(KoreanWon), snapshot.TotalPrice);
    }

    [Fact]
    public void Equivalent_price_snapshots_have_structural_value_equality()
    {
        StayDateRange firstStay = CreateTwoNightStay();
        StayDateRange secondStay = CreateTwoNightStay();

        ReservationPriceSnapshot first = CreateSnapshot(
            firstStay,
            CreateNightlyPrices(firstStay));
        ReservationPriceSnapshot second = CreateSnapshot(
            secondStay,
            CreateNightlyPrices(secondStay));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Price_snapshots_with_different_agreed_prices_are_not_equal()
    {
        StayDateRange stay = CreateTwoNightStay();
        ReservationPriceSnapshot original = CreateSnapshot(
            stay,
            CreateNightlyPrices(stay));
        ReservationPriceSnapshot changed = CreateSnapshot(
            stay,
            [
                CreateNightlyPrice(stay.CheckInDate, 150_000m),
                CreateNightlyPrice(stay.CheckInDate.AddDays(1), 190_000m)
            ]);

        Assert.NotEqual(original, changed);
    }

    [Fact]
    public void Create_rejects_a_default_price_quote_identifier()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                default,
                ExistingHotelId,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                stay,
                RequestedOccupancy.Create(2, 0),
                CreateNightlyPrices(stay),
                CreateCancellationPolicy()));

        Assert.Equal("priceQuoteId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_default_hotel_identifier()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                default,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                stay,
                RequestedOccupancy.Create(2, 0),
                CreateNightlyPrices(stay),
                CreateCancellationPolicy()));

        Assert.Equal("hotelId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_default_room_type_identifier()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                default,
                ExistingRatePlanId,
                stay,
                RequestedOccupancy.Create(2, 0),
                CreateNightlyPrices(stay),
                CreateCancellationPolicy()));

        Assert.Equal("roomTypeId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_default_rate_plan_identifier()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                ExistingRoomTypeId,
                default,
                stay,
                RequestedOccupancy.Create(2, 0),
                CreateNightlyPrices(stay),
                CreateCancellationPolicy()));

        Assert.Equal("ratePlanId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_stay_date_range()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                null!,
                RequestedOccupancy.Create(2, 0),
                [],
                CreateCancellationPolicy()));

        Assert.Equal("stayDateRange", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_requested_occupancy()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                stay,
                null!,
                CreateNightlyPrices(stay),
                CreateCancellationPolicy()));

        Assert.Equal("requestedOccupancy", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_nightly_price_collection()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                stay,
                RequestedOccupancy.Create(2, 0),
                null!,
                CreateCancellationPolicy()));

        Assert.Equal("nightlyPrices", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_an_empty_nightly_price_collection()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainArgumentException>(() => CreateSnapshot(stay, []));
    }

    [Fact]
    public void Create_rejects_a_missing_nightly_price()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainArgumentException>(() => CreateSnapshot(stay, [null!]));
    }

    [Fact]
    public void Create_rejects_a_missing_cancellation_policy()
    {
        StayDateRange stay = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationPriceSnapshot.Create(
                ExistingPriceQuoteId,
                ExistingHotelId,
                ExistingRoomTypeId,
                ExistingRatePlanId,
                stay,
                RequestedOccupancy.Create(2, 0),
                CreateNightlyPrices(stay),
                null!));

        Assert.Equal("cancellationPolicy", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_occupied_date()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationNightlyPrice.Create(default, Money.Zero(KoreanWon)));

        Assert.Equal("occupiedDate", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_nightly_money_value()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationNightlyPrice.Create(new DateOnly(2026, 8, 15), null!));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_occupied_date_price()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreateSnapshot(
                stay,
                [CreateNightlyPrice(stay.CheckInDate, 150_000m)]));
    }

    [Fact]
    public void Create_rejects_duplicate_occupied_date_prices()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreateSnapshot(
                stay,
                [
                    CreateNightlyPrice(stay.CheckInDate, 150_000m),
                    CreateNightlyPrice(stay.CheckInDate, 180_000m)
                ]));
    }

    [Fact]
    public void Create_rejects_a_price_outside_the_stay()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreateSnapshot(
                stay,
                [
                    CreateNightlyPrice(stay.CheckInDate, 150_000m),
                    CreateNightlyPrice(stay.CheckOutDate, 180_000m)
                ]));
    }

    [Fact]
    public void Create_rejects_nightly_prices_in_different_currencies()
    {
        StayDateRange stay = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreateSnapshot(
                stay,
                [
                    CreateNightlyPrice(stay.CheckInDate, 150_000m),
                    ReservationNightlyPrice.Create(
                        stay.CheckInDate.AddDays(1),
                        Money.Create(180m, Currency.FromCode("USD")))
                ]));
    }

    private static ReservationPriceSnapshot CreateSnapshot(
        StayDateRange stay,
        IReadOnlyCollection<ReservationNightlyPrice> nightlyPrices)
    {
        return ReservationPriceSnapshot.Create(
            ExistingPriceQuoteId,
            ExistingHotelId,
            ExistingRoomTypeId,
            ExistingRatePlanId,
            stay,
            RequestedOccupancy.Create(2, 0),
            nightlyPrices,
            CreateCancellationPolicy());
    }

    private static StayDateRange CreateTwoNightStay()
    {
        return new StayDateRange(
            new DateOnly(2026, 8, 15),
            new DateOnly(2026, 8, 17));
    }

    private static IReadOnlyCollection<ReservationNightlyPrice> CreateNightlyPrices(
        StayDateRange stay)
    {
        return
        [
            CreateNightlyPrice(stay.CheckInDate, 150_000m),
            CreateNightlyPrice(stay.CheckInDate.AddDays(1), 180_000m)
        ];
    }

    private static ReservationNightlyPrice CreateNightlyPrice(
        DateOnly occupiedDate,
        decimal amount)
    {
        return ReservationNightlyPrice.Create(
            occupiedDate,
            Money.Create(amount, KoreanWon));
    }

    private static CancellationPolicySnapshot CreateCancellationPolicy()
    {
        return CancellationPolicySnapshot.Create(
            new CancellationRuleSnapshot(
                TimeSpan.FromDays(3),
                CancellationPenaltySnapshot.NoPenalty()),
            new CancellationRuleSnapshot(
                TimeSpan.Zero,
                CancellationPenaltySnapshot.FullStay()));
    }
}
