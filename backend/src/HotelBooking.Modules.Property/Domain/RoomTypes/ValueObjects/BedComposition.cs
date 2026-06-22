using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

public sealed record BedComposition
{
    public BedType BedType { get; }
    public int Quantity { get; }

    private BedComposition(BedType bedType, int quantity)
    {
        BedType = bedType;
        Quantity = quantity;
    }

    public static BedComposition Create(BedType bedType, int quantity)
    {
        if (bedType == default || !Enum.IsDefined(typeof(BedType), bedType))
        {
            throw new DomainArgumentException("Bed type is unsupported.", nameof(bedType));
        }
        if (quantity < 1)
        {
            throw new DomainArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        return new BedComposition(bedType, quantity);
    }
}
