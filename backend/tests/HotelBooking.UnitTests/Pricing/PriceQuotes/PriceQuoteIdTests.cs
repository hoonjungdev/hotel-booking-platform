using HotelBooking.Modules.Pricing.Domain.PriceQuotes;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.PriceQuotes;

/// <summary>
/// Proves creation and reconstruction rules for Pricing-owned PriceQuote identifiers.
/// </summary>
public class PriceQuoteIdTests
{
    [Fact]
    public void Create_generates_non_empty_identifier()
    {
        PriceQuoteId priceQuoteId = PriceQuoteId.Create();

        Assert.NotEqual(Guid.Empty, priceQuoteId.Value);
    }

    [Fact]
    public void From_reconstructs_identifier()
    {
        Guid value = Guid.Parse("50000000-0000-0000-0000-000000000001");

        PriceQuoteId priceQuoteId = PriceQuoteId.From(value);

        Assert.Equal(value, priceQuoteId.Value);
    }

    [Fact]
    public void From_rejects_empty_identifier()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            PriceQuoteId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
