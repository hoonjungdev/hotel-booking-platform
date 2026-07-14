using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes;

/// <summary>Identifies a room type offered by a hotel.</summary>
public readonly record struct RoomTypeId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private RoomTypeId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("RoomTypeId cannot be empty", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new room type identifier.</summary>
    public static RoomTypeId Create()
    {
        return new RoomTypeId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a room type identifier from a persisted value.</summary>
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
