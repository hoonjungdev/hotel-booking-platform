using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans;

/// <summary>Identifies a sellable pricing option for a room type.</summary>
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

    /// <summary>Creates a new rate plan identifier.</summary>
    public static RatePlanId Create()
    {
        return new RatePlanId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a rate plan identifier from a persisted value.</summary>
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
