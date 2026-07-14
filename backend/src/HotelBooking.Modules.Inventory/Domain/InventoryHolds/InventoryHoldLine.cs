using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

public sealed class InventoryHoldLine : Entity<InventoryHoldLineId>
{
    public DateOnly OccupiedDate { get; private set; }
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
