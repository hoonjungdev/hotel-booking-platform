using HotelBooking.Modules.Pricing.Domain.DailyRates;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.DailyRates;

/// <summary>
/// Proves creation and reconstruction rules for Pricing-owned DailyRate identifiers.
/// </summary>
public class DailyRateIdTests
{
    [Fact]
    public void Create_generates_non_empty_identifier()
    {
        DailyRateId dailyRateId = DailyRateId.Create();

        Assert.NotEqual(Guid.Empty, dailyRateId.Value);
    }

    [Fact]
    public void From_reconstructs_identifier()
    {
        Guid value = Guid.Parse("40000000-0000-0000-0000-000000000001");

        DailyRateId dailyRateId = DailyRateId.From(value);

        Assert.Equal(value, dailyRateId.Value);
    }

    [Fact]
    public void From_rejects_empty_identifier()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            DailyRateId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
