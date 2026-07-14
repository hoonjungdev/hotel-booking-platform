using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Inventory.References;

public class ReservationIdTests
{
    [Fact]
    public void Create_creates_reservation_id()
    {
        ReservationId reservationId = ReservationId.Create();

        Assert.NotEqual(Guid.Empty, reservationId.Value);
    }

    [Fact]
    public void From_creates_reservation_id_from_guid()
    {
        var value = Guid.NewGuid();

        ReservationId reservationId = ReservationId.From(value);

        Assert.Equal(value, reservationId.Value);
    }

    [Fact]
    public void From_throws_when_value_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            ReservationId.From(Guid.Empty));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void ToString_returns_guid_value()
    {
        var value = Guid.NewGuid();
        ReservationId reservationId = ReservationId.From(value);

        Assert.Equal(value.ToString(), reservationId.ToString());
    }
}
