using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.References;

public class RoomTypeIdTests
{
    [Fact]
    public void Create_creates_non_empty_room_type_id()
    {
        RoomTypeId roomTypeId = RoomTypeId.Create();

        Assert.NotEqual(Guid.Empty, roomTypeId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RoomTypeId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
