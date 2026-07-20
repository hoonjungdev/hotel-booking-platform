using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.References;

/// <summary>References the Rate Plan agreed for a Reservation without depending on Pricing.</summary>
public readonly record struct RatePlanId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private RatePlanId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("RatePlanId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Reconstructs a Rate Plan reference from an identifier owned by Pricing.</summary>
    public static RatePlanId From(Guid value)
    {
        return new RatePlanId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
