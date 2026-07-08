using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.References;

public class HotelIdTests
{
    [Fact]
    public void Create_creates_non_empty_hotel_id()
    {
        HotelId hotelId = HotelId.Create();

        Assert.NotEqual(Guid.Empty, hotelId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            HotelId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
