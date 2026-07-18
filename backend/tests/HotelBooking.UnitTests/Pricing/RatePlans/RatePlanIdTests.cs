using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.RatePlans;

/// <summary>
/// Proves creation and reconstruction rules for Pricing-owned RatePlan identifiers.
/// </summary>
public class RatePlanIdTests
{
    [Fact]
    public void Create_returns_non_empty_identifier()
    {
        RatePlanId ratePlanId = RatePlanId.Create();

        Assert.NotEqual(Guid.Empty, ratePlanId.Value);
    }

    [Fact]
    public void From_preserves_identifier_value()
    {
        Guid value = Guid.Parse("10000000-0000-0000-0000-000000000002");

        RatePlanId ratePlanId = RatePlanId.From(value);

        Assert.Equal(value, ratePlanId.Value);
        Assert.Equal(value.ToString(), ratePlanId.ToString());
    }

    [Fact]
    public void From_rejects_empty_value()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RatePlanId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
