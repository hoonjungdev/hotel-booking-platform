using HotelBooking.Modules.Inventory.Domain.InventoryDates;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.InventoryDates;

public class InventoryDateIdTests
{
    [Fact]
    public void Create_creates_non_empty_inventory_date_id()
    {
        InventoryDateId inventoryDateId = InventoryDateId.Create();

        Assert.NotEqual(Guid.Empty, inventoryDateId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryDateId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
