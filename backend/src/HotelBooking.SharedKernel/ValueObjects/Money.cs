using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.SharedKernel.ValueObjects;

/// <summary>
/// Represents a non-negative monetary amount denominated in exactly one currency.
/// </summary>
public sealed record Money
{
    private Money(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>
    /// Gets the monetary quantity expressed in the associated currency.
    /// </summary>
    public decimal Amount { get; }

    /// <summary>
    /// Gets the denomination of the amount.
    /// </summary>
    public Currency Currency { get; }

    /// <summary>
    /// Creates a monetary amount in one currency.
    /// </summary>
    /// <param name="amount">The non-negative monetary quantity.</param>
    /// <param name="currency">The denomination of the amount.</param>
    /// <returns>The monetary amount.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the amount is negative or the currency is missing.
    /// </exception>
    public static Money Create(decimal amount, Currency currency)
    {
        if (amount < 0m)
        {
            throw new DomainArgumentException(
                "Money amount cannot be negative.",
                nameof(amount));
        }

        if (currency is null)
        {
            throw new DomainArgumentException(
                "Money currency is required.",
                nameof(currency));
        }

        return new Money(amount, currency);
    }

    /// <summary>
    /// Creates a zero monetary amount in a specific currency.
    /// </summary>
    /// <param name="currency">The denomination of the zero amount.</param>
    /// <returns>Zero money in the supplied currency.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the currency is missing.
    /// </exception>
    public static Money Zero(Currency currency)
    {
        return Create(0m, currency);
    }

    /// <summary>
    /// Adds another monetary amount denominated in the same currency.
    /// </summary>
    /// <param name="addend">The monetary amount to add.</param>
    /// <returns>A new monetary amount containing the sum.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the addend is missing or the monetary amounts use different currencies.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the sum exceeds the range supported by <see cref="decimal"/>.
    /// </exception>
    public Money Add(Money addend)
    {
        if (addend is null)
        {
            throw new DomainArgumentException(
                "Money addend is required.",
                nameof(addend));
        }

        if (Currency != addend.Currency)
        {
            throw new DomainArgumentException(
                "Money amounts in different currencies cannot be added.",
                nameof(addend));
        }

        return Create(Amount + addend.Amount, Currency);
    }
}
