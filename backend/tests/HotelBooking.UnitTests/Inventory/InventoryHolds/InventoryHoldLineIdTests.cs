using HotelBooking.Modules.Inventory.Domain.InventoryHolds;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.InventoryHolds;

public class InventoryHoldLineIdTests
{
    [Fact]
    public void Create_creates_inventory_hold_line_id()
    {
        InventoryHoldLineId inventoryHoldLineId = InventoryHoldLineId.Create();

        Assert.NotEqual(Guid.Empty, inventoryHoldLineId.Value);
    }

    [Fact]
    public void From_creates_inventory_hold_line_id_from_guid()
    {
        var value = Guid.NewGuid();

        InventoryHoldLineId inventoryHoldLineId = InventoryHoldLineId.From(value);

        Assert.Equal(value, inventoryHoldLineId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHoldLineId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void ToString_returns_guid_value()
    {
        var value = Guid.NewGuid();
        InventoryHoldLineId inventoryHoldLineId = InventoryHoldLineId.From(value);

        Assert.Equal(value.ToString(), inventoryHoldLineId.ToString());
    }
}
