using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

public readonly record struct InventoryHoldId
{
    public Guid Value { get; }

    private InventoryHoldId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryHoldId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static InventoryHoldId Create()
    {
        return new InventoryHoldId(Guid.NewGuid());
    }

    public static InventoryHoldId From(Guid value)
    {
        return new InventoryHoldId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
