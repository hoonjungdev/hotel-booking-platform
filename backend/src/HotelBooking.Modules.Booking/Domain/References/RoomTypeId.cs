using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.References;

/// <summary>References the Room Type selected for a Reservation without depending on Property.</summary>
public readonly record struct RoomTypeId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private RoomTypeId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("RoomTypeId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Reconstructs a Room Type reference from an identifier owned by Property.</summary>
    public static RoomTypeId From(Guid value)
    {
        return new RoomTypeId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
