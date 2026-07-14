using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

/// <summary>Identifies a temporary inventory claim for one reservation.</summary>
public readonly record struct InventoryHoldId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private InventoryHoldId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryHoldId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new inventory hold identifier.</summary>
    public static InventoryHoldId Create()
    {
        return new InventoryHoldId(Guid.NewGuid());
    }

    /// <summary>Rehydrates an inventory hold identifier from a persisted value.</summary>
    public static InventoryHoldId From(Guid value)
    {
        return new InventoryHoldId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
