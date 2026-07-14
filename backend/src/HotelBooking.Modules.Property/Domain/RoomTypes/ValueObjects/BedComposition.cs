using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

/// <summary>Represents a positive quantity of one bed type in a room type.</summary>
public sealed record BedComposition
{
    /// <summary>Gets the physical bed category.</summary>
    public BedType BedType { get; }
    /// <summary>Gets the number of beds of this category.</summary>
    public int Quantity { get; }

    private BedComposition(BedType bedType, int quantity)
    {
        BedType = bedType;
        Quantity = quantity;
    }

    /// <summary>Creates a supported bed composition with positive quantity.</summary>
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
