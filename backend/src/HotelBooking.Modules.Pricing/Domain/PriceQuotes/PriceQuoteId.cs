using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.PriceQuotes;

/// <summary>Identifies one time-limited pre-reservation price offer.</summary>
public readonly record struct PriceQuoteId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private PriceQuoteId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("PriceQuoteId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new PriceQuote identifier.</summary>
    /// <returns>A new non-empty identifier.</returns>
    public static PriceQuoteId Create()
    {
        return new PriceQuoteId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a PriceQuote identifier from a persisted value.</summary>
    /// <param name="value">The existing non-empty identifier value.</param>
    /// <returns>The reconstructed identifier.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the identifier is empty.</exception>
    public static PriceQuoteId From(Guid value)
    {
        return new PriceQuoteId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
