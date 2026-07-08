using HotelBooking.Modules.Inventory.Domain.InventoryDates;
using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.InventoryDates;

public class InventoryDateTests
{
    [Fact]
    public void Create_creates_inventory_date_with_available_quantity_equal_to_total_quantity()
    {
        var occupiedDate = new DateOnly(2026, 7, 1);

        InventoryDate inventoryDate = InventoryDate.Create(
            id: InventoryDateId.Create(),
            hotelId: HotelId.Create(),
            roomTypeId: RoomTypeId.Create(),
            occupiedDate: occupiedDate,
            totalQuantity: 5);

        Assert.Equal(occupiedDate, inventoryDate.OccupiedDate);
        Assert.Equal(5, inventoryDate.TotalQuantity);
        Assert.Equal(0, inventoryDate.HeldQuantity);
        Assert.Equal(0, inventoryDate.BookedQuantity);
        Assert.Equal(0, inventoryDate.ClosedQuantity);
        Assert.Equal(5, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void Create_throws_when_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDate.Create(
                id: default,
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                occupiedDate: new DateOnly(2026, 7, 1),
                totalQuantity: 5));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_throws_when_hotel_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDate.Create(
                id: InventoryDateId.Create(),
                hotelId: default,
                roomTypeId: RoomTypeId.Create(),
                occupiedDate: new DateOnly(2026, 7, 1),
                totalQuantity: 5));

        Assert.Equal("hotelId", exception.ParamName);
    }

    [Fact]
    public void Create_throws_when_room_type_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDate.Create(
                id: InventoryDateId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: default,
                occupiedDate: new DateOnly(2026, 7, 1),
                totalQuantity: 5));

        Assert.Equal("roomTypeId", exception.ParamName);
    }

    [Fact]
    public void Create_throws_when_occupied_date_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDate.Create(
                id: InventoryDateId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                occupiedDate: default,
                totalQuantity: 5));

        Assert.Equal("occupiedDate", exception.ParamName);
    }

    [Fact]
    public void Create_throws_when_total_quantity_is_negative()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDate.Create(
                id: InventoryDateId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                occupiedDate: new DateOnly(2026, 7, 1),
                totalQuantity: -1));

        Assert.Equal("totalQuantity", exception.ParamName);
    }

    [Fact]
    public void IncreaseHeldQuantity_increases_held_quantity_and_decreases_available_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 3);

        inventoryDate.IncreaseHeldQuantity(2);

        Assert.Equal(2, inventoryDate.HeldQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void IncreaseHeldQuantity_throws_when_available_quantity_is_insufficient()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 1);

        DomainException exception = Assert.Throws<DomainException>(() => inventoryDate.IncreaseHeldQuantity(2));

        Assert.Equal("Inventory date does not have enough available quantity to increase held quantity.", exception.Message);
        Assert.Equal(0, inventoryDate.HeldQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseHeldQuantity_throws_when_quantity_is_not_positive(int quantity)
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 1);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.IncreaseHeldQuantity(quantity));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void DecreaseHeldQuantity_decreases_held_quantity_and_increases_available_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseHeldQuantity(2);

        inventoryDate.DecreaseHeldQuantity(1);

        Assert.Equal(1, inventoryDate.HeldQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void DecreaseHeldQuantity_throws_when_held_quantity_is_insufficient()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);

        DomainException exception = Assert.Throws<DomainException>(() => inventoryDate.DecreaseHeldQuantity(1));

        Assert.Equal("Inventory date does not have enough held quantity to decrease.", exception.Message);
        Assert.Equal(0, inventoryDate.HeldQuantity);
        Assert.Equal(2, inventoryDate.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseHeldQuantity_throws_when_quantity_is_not_positive(int quantity)
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseHeldQuantity(1);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.DecreaseHeldQuantity(quantity));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void ConvertHeldToBookedQuantity_moves_quantity_from_held_to_booked()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseHeldQuantity(2);

        inventoryDate.ConvertHeldToBookedQuantity(1);

        Assert.Equal(1, inventoryDate.HeldQuantity);
        Assert.Equal(1, inventoryDate.BookedQuantity);
        Assert.Equal(0, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void ConvertHeldToBookedQuantity_throws_when_held_quantity_is_insufficient()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);

        DomainException exception = Assert.Throws<DomainException>(() => inventoryDate.ConvertHeldToBookedQuantity(1));

        Assert.Equal("Inventory date does not have enough held quantity to convert to booked quantity.", exception.Message);
        Assert.Equal(0, inventoryDate.HeldQuantity);
        Assert.Equal(0, inventoryDate.BookedQuantity);
        Assert.Equal(2, inventoryDate.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ConvertHeldToBookedQuantity_throws_when_quantity_is_not_positive(int quantity)
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseHeldQuantity(1);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.ConvertHeldToBookedQuantity(quantity));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void IncreaseClosedQuantity_increases_closed_quantity_and_decreases_available_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 3);

        inventoryDate.IncreaseClosedQuantity(2);

        Assert.Equal(2, inventoryDate.ClosedQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void IncreaseClosedQuantity_throws_when_available_quantity_is_insufficient()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 1);
        inventoryDate.IncreaseHeldQuantity(1);

        DomainException exception = Assert.Throws<DomainException>(() => inventoryDate.IncreaseClosedQuantity(1));

        Assert.Equal("Inventory date does not have enough available quantity to increase closed quantity.", exception.Message);
        Assert.Equal(0, inventoryDate.ClosedQuantity);
        Assert.Equal(0, inventoryDate.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IncreaseClosedQuantity_throws_when_quantity_is_not_positive(int quantity)
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.IncreaseClosedQuantity(quantity));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void DecreaseClosedQuantity_decreases_closed_quantity_and_increases_available_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseClosedQuantity(2);

        inventoryDate.DecreaseClosedQuantity(1);

        Assert.Equal(1, inventoryDate.ClosedQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void DecreaseClosedQuantity_throws_when_closed_quantity_is_insufficient()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);

        DomainException exception = Assert.Throws<DomainException>(() => inventoryDate.DecreaseClosedQuantity(1));

        Assert.Equal("Inventory date does not have enough closed quantity to decrease.", exception.Message);
        Assert.Equal(0, inventoryDate.ClosedQuantity);
        Assert.Equal(2, inventoryDate.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseClosedQuantity_throws_when_quantity_is_not_positive(int quantity)
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);
        inventoryDate.IncreaseClosedQuantity(1);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.DecreaseClosedQuantity(quantity));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void ChangeTotalQuantity_changes_total_when_new_total_can_cover_committed_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 5);
        inventoryDate.IncreaseHeldQuantity(2);
        inventoryDate.IncreaseClosedQuantity(1);

        inventoryDate.ChangeTotalQuantity(4);

        Assert.Equal(4, inventoryDate.TotalQuantity);
        Assert.Equal(1, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void ChangeTotalQuantity_throws_when_new_total_is_lower_than_committed_quantity()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 5);
        inventoryDate.IncreaseHeldQuantity(2);
        inventoryDate.IncreaseClosedQuantity(1);

        DomainException exception = Assert.Throws<DomainException>(() =>
            inventoryDate.ChangeTotalQuantity(2));

        Assert.Equal("Total quantity cannot be lower than held, booked, and closed quantities.", exception.Message);
        Assert.Equal(5, inventoryDate.TotalQuantity);
        Assert.Equal(2, inventoryDate.AvailableQuantity);
    }

    [Fact]
    public void ChangeTotalQuantity_throws_when_total_quantity_is_negative()
    {
        InventoryDate inventoryDate = CreateInventoryDate(totalQuantity: 2);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            inventoryDate.ChangeTotalQuantity(-1));

        Assert.Equal("totalQuantity", exception.ParamName);
        Assert.Equal(2, inventoryDate.TotalQuantity);
    }

    private static InventoryDate CreateInventoryDate(int totalQuantity)
    {
        return InventoryDate.Create(
            id: InventoryDateId.Create(),
            hotelId: HotelId.Create(),
            roomTypeId: RoomTypeId.Create(),
            occupiedDate: new DateOnly(2026, 7, 1),
            totalQuantity: totalQuantity);
    }
}
