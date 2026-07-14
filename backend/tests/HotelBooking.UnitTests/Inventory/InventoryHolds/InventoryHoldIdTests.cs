using HotelBooking.Modules.Inventory.Domain.InventoryHolds;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.InventoryHolds;

public class InventoryHoldIdTests
{
    [Fact]
    public void Create_creates_inventory_hold_id()
    {
        InventoryHoldId inventoryHoldId = InventoryHoldId.Create();

        Assert.NotEqual(Guid.Empty, inventoryHoldId.Value);
    }

    [Fact]
    public void From_creates_inventory_hold_id_from_guid()
    {
        var value = Guid.NewGuid();

        InventoryHoldId inventoryHoldId = InventoryHoldId.From(value);

        Assert.Equal(value, inventoryHoldId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHoldId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void ToString_returns_guid_value()
    {
        var value = Guid.NewGuid();
        InventoryHoldId inventoryHoldId = InventoryHoldId.From(value);

        Assert.Equal(value.ToString(), inventoryHoldId.ToString());
    }
}
