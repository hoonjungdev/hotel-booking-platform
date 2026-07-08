using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.Modules.Inventory.Domain.References;

namespace HotelBooking.Modules.Inventory.Domain.InventoryDates;

public sealed class InventoryDate : AggregateRoot<InventoryDateId>
{
    public HotelId HotelId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public DateOnly OccupiedDate { get; private set; }
    public int TotalQuantity { get; private set; }
    public int HeldQuantity { get; private set; }
    public int BookedQuantity { get; private set; }
    public int ClosedQuantity { get; private set; }
    public int AvailableQuantity => TotalQuantity - HeldQuantity - BookedQuantity - ClosedQuantity;

    private InventoryDate()
    {
        // Required by EF Core
    }

    private InventoryDate(
        InventoryDateId id,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        DateOnly occupiedDate,
        int totalQuantity)
    {
        Id = id;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        OccupiedDate = occupiedDate;
        TotalQuantity = totalQuantity;
    }

    public static InventoryDate Create(
        InventoryDateId id,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        DateOnly occupiedDate,
        int totalQuantity)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Inventory date ID is required.", nameof(id));
        }

        if (hotelId == default)
        {
            throw new DomainArgumentException("Hotel ID is required.", nameof(hotelId));
        }

        if (roomTypeId == default)
        {
            throw new DomainArgumentException("Room type ID is required.", nameof(roomTypeId));
        }

        if (occupiedDate == default)
        {
            throw new DomainArgumentException("Occupied date is required.", nameof(occupiedDate));
        }

        ValidateQuantityIsNotNegative(totalQuantity, nameof(totalQuantity));

        return new InventoryDate(
            id,
            hotelId,
            roomTypeId,
            occupiedDate,
            totalQuantity);
    }

    public void IncreaseHeldQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough available quantity to increase held quantity.");
        }

        HeldQuantity += quantity;
    }

    public void DecreaseHeldQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (HeldQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough held quantity to decrease.");
        }

        HeldQuantity -= quantity;
    }

    public void ConvertHeldToBookedQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (HeldQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough held quantity to convert to booked quantity.");
        }

        HeldQuantity -= quantity;
        BookedQuantity += quantity;
    }

    public void IncreaseClosedQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough available quantity to increase closed quantity.");
        }

        ClosedQuantity += quantity;
    }

    public void DecreaseClosedQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (ClosedQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough closed quantity to decrease.");
        }

        ClosedQuantity -= quantity;
    }

    public void ChangeTotalQuantity(int totalQuantity)
    {
        ValidateQuantityIsNotNegative(totalQuantity, nameof(totalQuantity));

        int committedQuantity = HeldQuantity + BookedQuantity + ClosedQuantity;
        if (totalQuantity < committedQuantity)
        {
            throw new DomainException("Total quantity cannot be lower than held, booked, and closed quantities.");
        }

        TotalQuantity = totalQuantity;
    }

    private static void ValidateQuantityIsPositive(int quantity, string parameterName)
    {
        if (quantity <= 0)
        {
            throw new DomainArgumentException("Quantity must be greater than zero.", parameterName);
        }
    }

    private static void ValidateQuantityIsNotNegative(int quantity, string parameterName)
    {
        if (quantity < 0)
        {
            throw new DomainArgumentException("Quantity cannot be negative.", parameterName);
        }
    }
}
