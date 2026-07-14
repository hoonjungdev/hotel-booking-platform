using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.Hotels;

/// <summary>Identifies a hotel managed by the Property module.</summary>
public readonly record struct HotelId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private HotelId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("HotelId cannot be empty", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new hotel identifier.</summary>
    public static HotelId Create()
    {
        return new HotelId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a hotel identifier from a persisted value.</summary>
    public static HotelId From(Guid value)
    {
        return new HotelId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
