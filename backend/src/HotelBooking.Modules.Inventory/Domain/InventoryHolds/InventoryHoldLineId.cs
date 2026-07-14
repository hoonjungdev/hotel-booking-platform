using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

/// <summary>Identifies an occupied-date line within an inventory hold.</summary>
public readonly record struct InventoryHoldLineId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private InventoryHoldLineId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryHoldLineId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new inventory hold line identifier.</summary>
    public static InventoryHoldLineId Create()
    {
        return new InventoryHoldLineId(Guid.NewGuid());
    }

    /// <summary>Rehydrates an inventory hold line identifier from a persisted value.</summary>
    public static InventoryHoldLineId From(Guid value)
    {
        return new InventoryHoldLineId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
