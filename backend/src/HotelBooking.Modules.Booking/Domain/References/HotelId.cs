using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.References;

/// <summary>References the Hotel selected for a Reservation without depending on Property.</summary>
public readonly record struct HotelId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private HotelId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("HotelId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Reconstructs a Hotel reference from an identifier owned by Property.</summary>
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
