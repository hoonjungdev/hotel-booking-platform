using HotelBooking.Modules.Pricing.Domain.DailyRates;
using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Pricing.DailyRates;

/// <summary>
/// Proves DailyRate creation and same-currency price change rules.
/// </summary>
public class DailyRateTests
{
    private static readonly DailyRateId ExistingDailyRateId =
        DailyRateId.From(Guid.Parse("40000000-0000-0000-0000-000000000001"));

    private static readonly RatePlanId ExistingRatePlanId =
        RatePlanId.From(Guid.Parse("10000000-0000-0000-0000-000000000001"));

    private static readonly HotelId ExistingHotelId =
        HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly RoomTypeId ExistingRoomTypeId =
        RoomTypeId.From(Guid.Parse("30000000-0000-0000-0000-000000000001"));

    private static readonly Currency KoreanWon = Currency.FromCode("KRW");
    private static readonly DateOnly OccupiedDate = new(2026, 8, 15);

    [Fact]
    public void Create_assigns_rate_plan_date_and_price()
    {
        RatePlan ratePlan = CreateDraftRatePlan();
        Money price = Money.Create(150_000m, KoreanWon);

        DailyRate dailyRate = DailyRate.Create(
            ExistingDailyRateId,
            ratePlan,
            OccupiedDate,
            price);

        Assert.Equal(ExistingDailyRateId, dailyRate.Id);
        Assert.Equal(ExistingRatePlanId, dailyRate.RatePlanId);
        Assert.Equal(OccupiedDate, dailyRate.OccupiedDate);
        Assert.Equal(price, dailyRate.Price);
    }

    [Fact]
    public void Create_allows_zero_price_in_the_rate_plan_currency()
    {
        DailyRate dailyRate = DailyRate.Create(
            ExistingDailyRateId,
            CreateDraftRatePlan(),
            OccupiedDate,
            Money.Zero(KoreanWon));

        Assert.Equal(Money.Zero(KoreanWon), dailyRate.Price);
    }

    [Fact]
    public void Create_rejects_default_daily_rate_id()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRate.Create(
                new DailyRateId(),
                CreateDraftRatePlan(),
                OccupiedDate,
                Money.Create(150_000m, KoreanWon)));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_missing_rate_plan()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRate.Create(
                ExistingDailyRateId,
                null!,
                OccupiedDate,
                Money.Create(150_000m, KoreanWon)));

        Assert.Equal("ratePlan", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_default_occupied_date()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRate.Create(
                ExistingDailyRateId,
                CreateDraftRatePlan(),
                default,
                Money.Create(150_000m, KoreanWon)));

        Assert.Equal("occupiedDate", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_missing_price()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRate.Create(
                ExistingDailyRateId,
                CreateDraftRatePlan(),
                OccupiedDate,
                null!));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_price_in_a_different_currency_than_the_rate_plan()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRate.Create(
                ExistingDailyRateId,
                CreateDraftRatePlan(),
                OccupiedDate,
                Money.Create(100m, Currency.FromCode("USD"))));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void ChangePrice_replaces_price_in_the_same_currency()
    {
        DailyRate dailyRate = CreateDailyRate();
        Money changedPrice = Money.Create(175_000m, KoreanWon);

        dailyRate.ChangePrice(changedPrice);

        Assert.Equal(changedPrice, dailyRate.Price);
    }

    [Fact]
    public void ChangePrice_rejects_missing_price()
    {
        DailyRate dailyRate = CreateDailyRate();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            dailyRate.ChangePrice(null!));

        Assert.Equal("price", exception.ParamName);
    }

    [Fact]
    public void ChangePrice_rejects_another_currency_and_preserves_the_agreed_currency()
    {
        DailyRate dailyRate = CreateDailyRate();
        Money originalPrice = dailyRate.Price;

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            dailyRate.ChangePrice(Money.Create(100m, Currency.FromCode("USD"))));

        Assert.Equal("price", exception.ParamName);
        Assert.Equal(originalPrice, dailyRate.Price);
    }

    private static DailyRate CreateDailyRate()
    {
        return DailyRate.Create(
            ExistingDailyRateId,
            CreateDraftRatePlan(),
            OccupiedDate,
            Money.Create(150_000m, KoreanWon));
    }

    private static RatePlan CreateDraftRatePlan()
    {
        CancellationPolicy cancellationPolicy = CancellationPolicy.Create(
            new CancellationRule(TimeSpan.Zero, CancellationPenalty.NoPenalty()));

        return RatePlan.CreateDraft(
            ExistingRatePlanId,
            HotelRateSettings.Create(ExistingHotelId, KoreanWon),
            ExistingRoomTypeId,
            "Flexible Breakfast Included",
            RatePlanCode.Create("FLEX-BB"),
            cancellationPolicy);
    }
}
