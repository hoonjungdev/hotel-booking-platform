using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Pricing.References;

/// <summary>
/// Proves that Pricing carries a hotel's identifier and selling currency as one immutable value.
/// </summary>
public class HotelRateSettingsTests
{
    private static readonly HotelId ExistingHotelId =
        HotelId.From(Guid.Parse("20000000-0000-0000-0000-000000000001"));

    private static readonly Currency KoreanWon = Currency.FromCode("KRW");

    [Fact]
    public void Create_keeps_hotel_and_selling_currency_together()
    {
        HotelRateSettings settings = HotelRateSettings.Create(
            ExistingHotelId,
            KoreanWon);

        Assert.Equal(ExistingHotelId, settings.HotelId);
        Assert.Equal(KoreanWon, settings.SellingCurrency);
    }

    [Fact]
    public void Create_rejects_default_hotel_id()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            HotelRateSettings.Create(new HotelId(), KoreanWon));

        Assert.Equal("hotelId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_missing_selling_currency()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            HotelRateSettings.Create(ExistingHotelId, null!));

        Assert.Equal("sellingCurrency", exception.ParamName);
    }

    [Fact]
    public void Equal_hotel_rate_settings_have_structural_equality()
    {
        HotelRateSettings first = HotelRateSettings.Create(ExistingHotelId, KoreanWon);
        HotelRateSettings second = HotelRateSettings.Create(
            ExistingHotelId,
            Currency.FromCode("krw"));

        Assert.Equal(first, second);
    }
}
