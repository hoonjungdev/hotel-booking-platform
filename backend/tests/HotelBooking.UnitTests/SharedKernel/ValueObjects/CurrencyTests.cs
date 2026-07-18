using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.SharedKernel.ValueObjects;

/// <summary>
/// Proves the canonical representation and validity rules of a currency.
/// </summary>
public class CurrencyTests
{
    [Fact]
    public void Normalizes_three_letter_currency_code()
    {
        Currency currency = Currency.FromCode("  krw  ");

        Assert.Equal("KRW", currency.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("US")]
    [InlineData("EURO")]
    [InlineData("U5D")]
    [InlineData("원화원")]
    [InlineData("uſd")]
    public void Rejects_invalid_currency_code(string? code)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(
            () => Currency.FromCode(code!));

        Assert.Equal("code", exception.ParamName);
    }
}
