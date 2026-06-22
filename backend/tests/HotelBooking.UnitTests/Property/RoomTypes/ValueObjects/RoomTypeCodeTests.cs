using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.RoomTypes.ValueObjects;

public class RoomTypeCodeTests
{
    [Fact]
    public void Create_trims_and_uppercases_code()
    {
        var code = RoomTypeCode.Create("   standard   ");

        Assert.Equal("STANDARD", code.Value);
    }

    [Fact]
    public void Create_throws_when_code_is_empty()
    {
        Assert.Throws<DomainArgumentException>(() => RoomTypeCode.Create(" "));
    }

    [Fact]
    public void Create_throws_when_code_is_too_long()
    {
        Assert.Throws<DomainArgumentException>(() => RoomTypeCode.Create(new string('a', 31)));
    }

    [Fact]
    public void ToString_returns_code()
    {
        var code = RoomTypeCode.Create(" standard  ");

        Assert.Equal("STANDARD", code.ToString());
    }
}
