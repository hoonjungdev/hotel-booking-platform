using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

/// <summary>
/// Records the quantity claimed by an inventory hold on one occupied date.
/// </summary>
public sealed class InventoryHoldLine : Entity<InventoryHoldLineId>
{
    /// <summary>Gets the inventory-consuming date.</summary>
    public DateOnly OccupiedDate { get; private set; }
    /// <summary>Gets the quantity claimed on the occupied date.</summary>
    public int Quantity { get; private set; }

    private InventoryHoldLine()
    {
        // Required by EF Core
    }

    private InventoryHoldLine(
        InventoryHoldLineId id,
        DateOnly occupiedDate,
        int quantity)
    {
        Id = id;
        OccupiedDate = occupiedDate;
        Quantity = quantity;
    }

    /// <summary>
    /// Creates a line for a positive quantity on one occupied date.
    /// </summary>
    public static InventoryHoldLine Create(
        InventoryHoldLineId id,
        DateOnly occupiedDate,
        int quantity)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Inventory hold line ID is required.", nameof(id));
        }

        if (occupiedDate == default)
        {
            throw new DomainArgumentException("Occupied date is required.", nameof(occupiedDate));
        }

        if (quantity <= 0)
        {
            throw new DomainArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        return new InventoryHoldLine(id, occupiedDate, quantity);
    }
}
