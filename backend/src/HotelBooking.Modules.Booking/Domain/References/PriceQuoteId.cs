using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.References;

/// <summary>References the Price Quote accepted for a Reservation without depending on Pricing.</summary>
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

    /// <summary>Reconstructs a Price Quote reference from an identifier owned by Pricing.</summary>
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
