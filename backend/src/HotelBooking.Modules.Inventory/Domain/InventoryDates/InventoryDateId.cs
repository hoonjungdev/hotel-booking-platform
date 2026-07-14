using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryDates;

/// <summary>Identifies one hotel, room type, and occupied-date inventory record.</summary>
public readonly record struct InventoryDateId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private InventoryDateId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("InventoryDateId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new inventory date identifier.</summary>
    public static InventoryDateId Create()
    {
        return new InventoryDateId(Guid.NewGuid());
    }

    /// <summary>Rehydrates an inventory date identifier from a persisted value.</summary>
    public static InventoryDateId From(Guid value)
    {
        return new InventoryDateId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
