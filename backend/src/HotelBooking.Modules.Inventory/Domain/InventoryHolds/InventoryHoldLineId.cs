using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

public readonly record struct InventoryHoldLineId
{
    public Guid Value { get; }

    private InventoryHoldLineId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryHoldLineId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    public static InventoryHoldLineId Create()
    {
        return new InventoryHoldLineId(Guid.NewGuid());
    }

    public static InventoryHoldLineId From(Guid value)
    {
        return new InventoryHoldLineId(value);
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}
