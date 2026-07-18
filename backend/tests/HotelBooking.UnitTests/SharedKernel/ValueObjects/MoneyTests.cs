using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.SharedKernel.ValueObjects;

/// <summary>
/// Proves the monetary amount and same-currency combination rules.
/// </summary>
public class MoneyTests
{
    [Fact]
    public void Creates_non_negative_amount_in_one_currency()
    {
        Currency currency = Currency.FromCode("KRW");

        Money money = Money.Create(150_000m, currency);

        Assert.Equal(150_000m, money.Amount);
        Assert.Equal(currency, money.Currency);
    }

    [Fact]
    public void Rejects_negative_amount()
    {
        Currency currency = Currency.FromCode("KRW");

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(
            () => Money.Create(-0.01m, currency));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void Rejects_amount_without_currency()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(
            () => Money.Create(150_000m, null!));

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Creates_zero_in_a_specific_currency()
    {
        Currency currency = Currency.FromCode("USD");

        Money zero = Money.Zero(currency);

        Assert.Equal(Money.Create(0m, currency), zero);
    }

    [Fact]
    public void Adds_amounts_in_the_same_currency()
    {
        Money roomRate = Money.Create(150_000m, Currency.FromCode("krw"));
        Money extraCharge = Money.Create(10_000m, Currency.FromCode("KRW"));

        Money total = roomRate.Add(extraCharge);

        Assert.Equal(
            Money.Create(160_000m, Currency.FromCode("KRW")),
            total);
    }

    [Fact]
    public void Rejects_adding_amounts_in_different_currencies()
    {
        Money koreanWon = Money.Create(150_000m, Currency.FromCode("KRW"));
        Money usDollars = Money.Create(100m, Currency.FromCode("USD"));

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(
            () => koreanWon.Add(usDollars));

        Assert.Equal("addend", exception.ParamName);
    }

    [Fact]
    public void Rejects_adding_a_missing_amount()
    {
        Money money = Money.Create(150_000m, Currency.FromCode("KRW"));

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(
            () => money.Add(null!));

        Assert.Equal("addend", exception.ParamName);
    }

    [Fact]
    public void Throws_when_same_currency_sum_exceeds_supported_range()
    {
        Currency currency = Currency.FromCode("USD");
        Money maximumAmount = Money.Create(decimal.MaxValue, currency);
        Money additionalAmount = Money.Create(1m, currency);

        Assert.Throws<OverflowException>(() => maximumAmount.Add(additionalAmount));
    }
}
