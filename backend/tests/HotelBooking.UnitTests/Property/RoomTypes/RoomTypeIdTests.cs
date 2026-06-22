using HotelBooking.Modules.Property.Domain.RoomTypes;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.RoomTypes;

public class RoomTypeIdTests
{
    [Fact]
    public void Create_returns_non_empty_id()
    {
        var id = RoomTypeId.Create();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void Create_returns_unique_ids()
    {
        var id1 = RoomTypeId.Create();
        var id2 = RoomTypeId.Create();

        Assert.NotEqual(id1.Value, id2.Value);
    }

    [Fact]
    public void ToString_returns_underlying_guid()
    {
        var id = RoomTypeId.Create();

        Assert.Equal(id.Value.ToString(), id.ToString());
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => RoomTypeId.From(Guid.Empty));
    }

    [Fact]
    public void From_returns_expected_value()
    {
        var value = Guid.NewGuid();
        var id = RoomTypeId.From(value);

        Assert.Equal(value, id.Value);
    }
}
