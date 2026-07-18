using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.DailyRates;

/// <summary>Identifies one occupied-date price under a RatePlan.</summary>
public readonly record struct DailyRateId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private DailyRateId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("DailyRateId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new DailyRate identifier.</summary>
    /// <returns>A new non-empty identifier.</returns>
    public static DailyRateId Create()
    {
        return new DailyRateId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a DailyRate identifier from a persisted value.</summary>
    /// <param name="value">The existing non-empty identifier value.</param>
    /// <returns>The reconstructed identifier.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the identifier is empty.</exception>
    public static DailyRateId From(Guid value)
    {
        return new DailyRateId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
