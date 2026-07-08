using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryDates;

public readonly record struct InventoryDateId
{
    public Guid Value { get; }

    private InventoryDateId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryDateId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static InventoryDateId Create()
    {
        return new InventoryDateId(Guid.NewGuid());
    }

    public static InventoryDateId From(Guid value)
    {
        return new InventoryDateId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
