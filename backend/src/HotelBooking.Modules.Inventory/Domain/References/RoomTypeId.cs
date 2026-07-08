using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.References;

public readonly record struct RoomTypeId
{
    public Guid Value { get; }

    private RoomTypeId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("RoomTypeId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static RoomTypeId Create()
    {
        return new RoomTypeId(Guid.NewGuid());
    }

    public static RoomTypeId From(Guid value)
    {
        return new RoomTypeId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
