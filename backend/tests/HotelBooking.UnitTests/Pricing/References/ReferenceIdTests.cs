using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.References;

/// <summary>
/// Proves reconstruction rules for identifiers owned by modules outside Pricing.
/// </summary>
public class ReferenceIdTests
{
    [Fact]
    public void HotelId_from_preserves_existing_identifier()
    {
        Guid value = Guid.Parse("20000000-0000-0000-0000-000000000002");

        HotelId hotelId = HotelId.From(value);

        Assert.Equal(value, hotelId.Value);
        Assert.Equal(value.ToString(), hotelId.ToString());
    }

    [Fact]
    public void HotelId_from_rejects_empty_value()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            HotelId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void RoomTypeId_from_preserves_existing_identifier()
    {
        Guid value = Guid.Parse("30000000-0000-0000-0000-000000000002");

        RoomTypeId roomTypeId = RoomTypeId.From(value);

        Assert.Equal(value, roomTypeId.Value);
        Assert.Equal(value.ToString(), roomTypeId.ToString());
    }

    [Fact]
    public void RoomTypeId_from_rejects_empty_value()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RoomTypeId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }
}
