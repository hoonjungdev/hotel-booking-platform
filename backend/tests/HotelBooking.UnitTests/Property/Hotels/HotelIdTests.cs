using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.Hotels;

public class HotelIdTests
{
    [Fact]
    public void Create_returns_non_empty_id()
    {
        var hotelId = HotelId.Create();

        Assert.NotEqual(Guid.Empty, hotelId.Value);
    }

    [Fact]
    public void Create_returns_unique_ids()
    {
        var hotelId1 = HotelId.Create();
        var hotelId2 = HotelId.Create();

        Assert.NotEqual(hotelId1, hotelId2);
    }

    [Fact]
    public void ToString_returns_underlying_guid()
    {
        var hotelId = HotelId.Create();

        Assert.Equal(hotelId.Value.ToString(), hotelId.ToString());
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => HotelId.From(Guid.Empty));
    }

    [Fact]
    public void From_returns_expected_value()
    {
        var value = Guid.NewGuid();
        var hotelId = HotelId.From(value);

        Assert.Equal(value, hotelId.Value);
    }
}
