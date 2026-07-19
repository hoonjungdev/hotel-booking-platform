using HotelBooking.Modules.Pricing.Domain.DailyRates;
using HotelBooking.Modules.Pricing.Domain.PriceQuotes;
using HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Pricing.PriceQuotes;

/// <summary>
/// Proves PriceQuote completeness, consistency, calculation, snapshot, and expiration rules.
/// </summary>
public class PriceQuoteTests
{
    private static readonly PriceQuoteId ExistingPriceQuoteId =
        PriceQuoteId.From(Guid.Parse("50000000-0000-0000-0000-000000000001"));

    private static readonly RatePlanId ExistingRatePlanId =
        RatePlanId.From(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly HotelId ExistingHotelId =
        HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly RoomTypeId ExistingRoomTypeId =
        RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static readonly Currency KoreanWon = Currency.FromCode("KRW");
    private static readonly DateTimeOffset QuotedAt =
        new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpiresAt = QuotedAt.AddMinutes(15);

    [Fact]
    public void Create_preserves_an_ordered_multi_night_offer_and_calculates_its_total()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();
        RequestedOccupancy requestedOccupancy = RequestedOccupancy.Create(2, 1);
        DailyRate firstNight = CreateDailyRate(
            ratePlan,
            stayDateRange.CheckInDate,
            150_000m,
            1);
        DailyRate secondNight = CreateDailyRate(
            ratePlan,
            stayDateRange.CheckInDate.AddDays(1),
            180_000m,
            2);

        PriceQuote quote = PriceQuote.Create(
            ExistingPriceQuoteId,
            ratePlan,
            stayDateRange,
            requestedOccupancy,
            [secondNight, firstNight],
            QuotedAt,
            ExpiresAt);

        Assert.Equal(ExistingPriceQuoteId, quote.Id);
        Assert.Equal(ExistingHotelId, quote.HotelId);
        Assert.Equal(ExistingRoomTypeId, quote.RoomTypeId);
        Assert.Equal(ExistingRatePlanId, quote.RatePlanId);
        Assert.Equal(stayDateRange, quote.StayDateRange);
        Assert.Equal(requestedOccupancy, quote.RequestedOccupancy);
        Assert.Equal(QuotedAt, quote.QuotedAt);
        Assert.Equal(ExpiresAt, quote.ExpiresAt);
        Assert.Collection(
            quote.NightlyPrices,
            nightlyPrice =>
            {
                Assert.Equal(stayDateRange.CheckInDate, nightlyPrice.OccupiedDate);
                Assert.Equal(Money.Create(150_000m, KoreanWon), nightlyPrice.Price);
            },
            nightlyPrice =>
            {
                Assert.Equal(stayDateRange.CheckInDate.AddDays(1), nightlyPrice.OccupiedDate);
                Assert.Equal(Money.Create(180_000m, KoreanWon), nightlyPrice.Price);
            });
        Assert.Equal(Money.Create(330_000m, KoreanWon), quote.TotalPrice);
        Assert.Equal(ratePlan.CancellationPolicy, quote.CancellationPolicy);
        Assert.NotSame(ratePlan.CancellationPolicy, quote.CancellationPolicy);
    }

    [Fact]
    public void Create_allows_a_single_free_night()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = new(
            new DateOnly(2026, 8, 15),
            new DateOnly(2026, 8, 16));

        PriceQuote quote = CreatePriceQuote(
            ratePlan,
            stayDateRange,
            [CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 0m, 1)]);

        NightlyPrice nightlyPrice = Assert.Single(quote.NightlyPrices);
        Assert.Equal(Money.Zero(KoreanWon), nightlyPrice.Price);
        Assert.Equal(Money.Zero(KoreanWon), quote.TotalPrice);
    }

    [Fact]
    public void Create_rejects_a_default_identifier()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                new PriceQuoteId(),
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                CreateDailyRates(ratePlan, stayDateRange),
                QuotedAt,
                ExpiresAt));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_rate_plan()
    {
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                null!,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                [],
                QuotedAt,
                ExpiresAt));

        Assert.Equal("ratePlan", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_stay_date_range()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                null!,
                RequestedOccupancy.Create(2, 0),
                [],
                QuotedAt,
                ExpiresAt));

        Assert.Equal("stayDateRange", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_requested_occupancy()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                null!,
                CreateDailyRates(ratePlan, stayDateRange),
                QuotedAt,
                ExpiresAt));

        Assert.Equal("requestedOccupancy", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_daily_rate_collection()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                null!,
                QuotedAt,
                ExpiresAt));

        Assert.Equal("dailyRates", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_daily_rate_in_the_collection()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                [null!],
                QuotedAt,
                ExpiresAt));

        Assert.Equal("dailyRates", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_issue_time()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                CreateDailyRates(ratePlan, stayDateRange),
                default,
                ExpiresAt));

        Assert.Equal("quotedAt", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_expiration_time()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                CreateDailyRates(ratePlan, stayDateRange),
                QuotedAt,
                default));

        Assert.Equal("expiresAt", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_expiration_that_is_not_after_issue_time(int minutesAfterIssue)
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            PriceQuote.Create(
                ExistingPriceQuoteId,
                ratePlan,
                stayDateRange,
                RequestedOccupancy.Create(2, 0),
                CreateDailyRates(ratePlan, stayDateRange),
                QuotedAt,
                QuotedAt.AddMinutes(minutesAfterIssue)));
    }

    [Theory]
    [InlineData(RatePlanStatus.Draft)]
    [InlineData(RatePlanStatus.Suspended)]
    [InlineData(RatePlanStatus.Closed)]
    public void Create_rejects_an_inactive_rate_plan(RatePlanStatus status)
    {
        RatePlan ratePlan = CreateRatePlan(status);
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(ratePlan, stayDateRange, CreateDailyRates(ratePlan, stayDateRange)));
    }

    [Fact]
    public void Create_rejects_a_stay_with_a_missing_daily_rate()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(
                ratePlan,
                stayDateRange,
                [CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 150_000m, 1)]));
    }

    [Fact]
    public void Create_rejects_duplicate_daily_rates_for_one_occupied_date()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(
                ratePlan,
                stayDateRange,
                [
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 150_000m, 1),
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 160_000m, 2),
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate.AddDays(1), 180_000m, 3)
                ]));
    }

    [Fact]
    public void Create_rejects_a_daily_rate_outside_the_stay()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();
        List<DailyRate> dailyRates = CreateDailyRates(ratePlan, stayDateRange).ToList();
        dailyRates.Add(CreateDailyRate(ratePlan, stayDateRange.CheckOutDate, 200_000m, 3));

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(ratePlan, stayDateRange, dailyRates));
    }

    [Fact]
    public void Create_rejects_a_daily_rate_from_another_rate_plan()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        RatePlan anotherRatePlan = CreateRatePlan(
            RatePlanStatus.Active,
            RatePlanId.From(Guid.Parse("10000000-0000-0000-0000-000000000002")));
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(
                ratePlan,
                stayDateRange,
                [
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 150_000m, 1),
                    CreateDailyRate(
                        anotherRatePlan,
                        stayDateRange.CheckInDate.AddDays(1),
                        180_000m,
                        2)
                ]));
    }

    [Fact]
    public void Create_rejects_a_daily_rate_in_another_currency()
    {
        RatePlan koreanWonRatePlan = CreateRatePlan(RatePlanStatus.Active);
        RatePlan usdRatePlanWithSameId = CreateRatePlan(
            RatePlanStatus.Active,
            ExistingRatePlanId,
            Currency.FromCode("USD"));
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<DomainException>(() =>
            CreatePriceQuote(
                koreanWonRatePlan,
                stayDateRange,
                CreateDailyRates(usdRatePlanWithSameId, stayDateRange)));
    }

    [Fact]
    public void Create_copies_daily_rates_before_the_caller_can_change_its_collection()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();
        List<DailyRate> dailyRates = CreateDailyRates(ratePlan, stayDateRange).ToList();
        PriceQuote quote = CreatePriceQuote(ratePlan, stayDateRange, dailyRates);

        dailyRates.Clear();

        Assert.Equal(2, quote.NightlyPrices.Count);
        Assert.Equal(Money.Create(330_000m, KoreanWon), quote.TotalPrice);
    }

    [Fact]
    public void Issued_quote_keeps_its_price_when_a_source_daily_rate_changes()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();
        List<DailyRate> dailyRates = CreateDailyRates(ratePlan, stayDateRange).ToList();
        PriceQuote quote = CreatePriceQuote(ratePlan, stayDateRange, dailyRates);

        dailyRates[0].ChangePrice(Money.Create(999_000m, KoreanWon));

        Assert.Equal(Money.Create(150_000m, KoreanWon), quote.NightlyPrices[0].Price);
        Assert.Equal(Money.Create(330_000m, KoreanWon), quote.TotalPrice);
    }

    [Fact]
    public void Nightly_prices_cannot_be_changed_through_the_exposed_collection()
    {
        PriceQuote quote = CreateValidPriceQuote();
        IList<NightlyPrice> exposedPrices = Assert.IsAssignableFrom<IList<NightlyPrice>>(
            quote.NightlyPrices);

        Assert.Throws<NotSupportedException>(() =>
            exposedPrices.Add(quote.NightlyPrices[0]));
    }

    [Fact]
    public void Create_propagates_an_overflow_when_the_total_cannot_be_represented()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        Assert.Throws<OverflowException>(() =>
            CreatePriceQuote(
                ratePlan,
                stayDateRange,
                [
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate, decimal.MaxValue, 1),
                    CreateDailyRate(ratePlan, stayDateRange.CheckInDate.AddDays(1), 1m, 2)
                ]));
    }

    [Fact]
    public void IsExpiredAt_returns_false_immediately_before_expiration()
    {
        PriceQuote quote = CreateValidPriceQuote();

        bool isExpired = quote.IsExpiredAt(ExpiresAt.AddTicks(-1));

        Assert.False(isExpired);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void IsExpiredAt_returns_true_at_or_after_expiration(long ticksAfterExpiration)
    {
        PriceQuote quote = CreateValidPriceQuote();

        bool isExpired = quote.IsExpiredAt(ExpiresAt.AddTicks(ticksAfterExpiration));

        Assert.True(isExpired);
    }

    [Fact]
    public void IsExpiredAt_rejects_a_missing_evaluation_time()
    {
        PriceQuote quote = CreateValidPriceQuote();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            quote.IsExpiredAt(default));

        Assert.Equal("now", exception.ParamName);
    }

    private static PriceQuote CreateValidPriceQuote()
    {
        RatePlan ratePlan = CreateRatePlan(RatePlanStatus.Active);
        StayDateRange stayDateRange = CreateTwoNightStay();

        return CreatePriceQuote(ratePlan, stayDateRange, CreateDailyRates(ratePlan, stayDateRange));
    }

    private static PriceQuote CreatePriceQuote(
        RatePlan ratePlan,
        StayDateRange stayDateRange,
        IReadOnlyCollection<DailyRate> dailyRates)
    {
        return PriceQuote.Create(
            ExistingPriceQuoteId,
            ratePlan,
            stayDateRange,
            RequestedOccupancy.Create(2, 0),
            dailyRates,
            QuotedAt,
            ExpiresAt);
    }

    private static IReadOnlyCollection<DailyRate> CreateDailyRates(
        RatePlan ratePlan,
        StayDateRange stayDateRange)
    {
        return
        [
            CreateDailyRate(ratePlan, stayDateRange.CheckInDate, 150_000m, 1),
            CreateDailyRate(ratePlan, stayDateRange.CheckInDate.AddDays(1), 180_000m, 2)
        ];
    }

    private static DailyRate CreateDailyRate(
        RatePlan ratePlan,
        DateOnly occupiedDate,
        decimal amount,
        int identifierSuffix)
    {
        return DailyRate.Create(
            DailyRateId.From(Guid.Parse($"40000000-0000-0000-0000-{identifierSuffix:D12}")),
            ratePlan,
            occupiedDate,
            Money.Create(amount, ratePlan.SellingCurrency));
    }

    private static StayDateRange CreateTwoNightStay()
    {
        return new StayDateRange(
            new DateOnly(2026, 8, 15),
            new DateOnly(2026, 8, 17));
    }

    private static RatePlan CreateRatePlan(
        RatePlanStatus status,
        RatePlanId? id = null,
        Currency? sellingCurrency = null)
    {
        RatePlan ratePlan = RatePlan.CreateDraft(
            id ?? ExistingRatePlanId,
            HotelRateSettings.Create(ExistingHotelId, sellingCurrency ?? KoreanWon),
            ExistingRoomTypeId,
            "Flexible Breakfast Included",
            RatePlanCode.Create("FLEX-BB"),
            CancellationPolicy.Create(
                new CancellationRule(
                    TimeSpan.FromDays(7),
                    CancellationPenalty.NoPenalty()),
                new CancellationRule(
                    TimeSpan.Zero,
                    CancellationPenalty.NumberOfNights(1))));

        switch (status)
        {
            case RatePlanStatus.Draft:
                return ratePlan;
            case RatePlanStatus.Active:
                ratePlan.Activate();
                return ratePlan;
            case RatePlanStatus.Suspended:
                ratePlan.Activate();
                ratePlan.Suspend();
                return ratePlan;
            case RatePlanStatus.Closed:
                ratePlan.Activate();
                ratePlan.Suspend();
                ratePlan.Close();
                return ratePlan;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}
