using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

/// <summary>References the room type whose quantity is managed without depending on the Property module.</summary>
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

    /// <summary>Creates a new room type reference identifier.</summary>
    public static RoomTypeId Create()
    {
        return new RoomTypeId(Guid.NewGuid());
    }

    /// <summary>Creates a room type reference from an existing identifier value.</summary>
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
