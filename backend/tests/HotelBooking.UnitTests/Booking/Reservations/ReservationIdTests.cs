using HotelBooking.Modules.Booking.Domain.Reservations;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Booking.Reservations;

/// <summary>Proves creation and reconstruction rules for Booking-owned Reservation identifiers.</summary>
public class ReservationIdTests
{
    [Fact]
    public void Create_returns_a_non_empty_identifier()
    {
        ReservationId id = ReservationId.Create();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void From_reconstructs_an_existing_identifier()
    {
        Guid value = Guid.Parse("20000000-0000-0000-0000-000000000001");

        ReservationId id = ReservationId.From(value);

        Assert.Equal(value, id.Value);
        Assert.Equal(value.ToString(), id.ToString());
    }

    [Fact]
    public void From_rejects_an_empty_identifier()
    {
        Assert.Throws<DomainArgumentException>(() => ReservationId.From(Guid.Empty));
    }
}
