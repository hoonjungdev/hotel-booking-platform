using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Inventory.Domain.InventoryDates;

/// <summary>
/// Governs sellable room type quantities for one hotel and occupied date.
/// </summary>
public sealed class InventoryDate : AggregateRoot<InventoryDateId>
{
    /// <summary>Gets the hotel that owns this inventory.</summary>
    public HotelId HotelId { get; private set; }
    /// <summary>Gets the room type whose quantity is managed.</summary>
    public RoomTypeId RoomTypeId { get; private set; }
    /// <summary>Gets the date on which the room type inventory is occupied.</summary>
    public DateOnly OccupiedDate { get; private set; }
    /// <summary>Gets the total manageable room quantity.</summary>
    public int TotalQuantity { get; private set; }
    /// <summary>Gets the quantity temporarily claimed while payment is pending.</summary>
    public int HeldQuantity { get; private set; }
    /// <summary>Gets the quantity committed to confirmed reservations.</summary>
    public int BookedQuantity { get; private set; }
    /// <summary>Gets the quantity intentionally removed from sale.</summary>
    public int ClosedQuantity { get; private set; }
    /// <summary>Gets the remaining quantity available for a new inventory hold.</summary>
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

    /// <summary>
    /// Creates an inventory date with no held, booked, or closed quantity.
    /// </summary>
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

    /// <summary>
    /// Claims available quantity for reservations awaiting payment.
    /// </summary>
    /// <param name="quantity">The positive quantity to hold.</param>
    /// <exception cref="DomainException">Thrown when available quantity is insufficient.</exception>
    public void IncreaseHeldQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough available quantity to increase held quantity.");
        }

        HeldQuantity += quantity;
    }

    /// <summary>
    /// Releases previously held quantity back to availability.
    /// </summary>
    public void DecreaseHeldQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (HeldQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough held quantity to decrease.");
        }

        HeldQuantity -= quantity;
    }

    /// <summary>
    /// Converts held quantity into booked quantity after reservation confirmation.
    /// </summary>
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

    /// <summary>
    /// Removes available quantity from sale for hotel operations.
    /// </summary>
    public void IncreaseClosedQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (AvailableQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough available quantity to increase closed quantity.");
        }

        ClosedQuantity += quantity;
    }

    /// <summary>
    /// Returns operationally closed quantity to availability.
    /// </summary>
    public void DecreaseClosedQuantity(int quantity)
    {
        ValidateQuantityIsPositive(quantity, nameof(quantity));

        if (ClosedQuantity < quantity)
        {
            throw new DomainException("Inventory date does not have enough closed quantity to decrease.");
        }

        ClosedQuantity -= quantity;
    }

    /// <summary>
    /// Changes total quantity without invalidating existing held, booked, or closed commitments.
    /// </summary>
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
