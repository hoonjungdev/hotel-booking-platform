using HotelBooking.Modules.Inventory.Domain.InventoryHolds;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.InventoryHolds;

public class InventoryHoldLineTests
{
    [Fact]
    public void Create_creates_inventory_hold_line()
    {
        var occupiedDate = new DateOnly(2026, 7, 1);

        InventoryHoldLine line = InventoryHoldLine.Create(
            id: InventoryHoldLineId.Create(),
            occupiedDate: occupiedDate,
            quantity: 2);

        Assert.Equal(occupiedDate, line.OccupiedDate);
        Assert.Equal(2, line.Quantity);
    }

    [Fact]
    public void Create_throws_when_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHoldLine.Create(
                id: default,
                occupiedDate: new DateOnly(2026, 7, 1),
                quantity: 1));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_throws_when_occupied_date_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHoldLine.Create(
                id: InventoryHoldLineId.Create(),
                occupiedDate: default,
                quantity: 1));

        Assert.Equal("occupiedDate", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_throws_when_quantity_is_not_positive(int quantity)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHoldLine.Create(
                id: InventoryHoldLineId.Create(),
                occupiedDate: new DateOnly(2026, 7, 1),
                quantity: quantity));

        Assert.Equal("quantity", exception.ParamName);
    }
}
