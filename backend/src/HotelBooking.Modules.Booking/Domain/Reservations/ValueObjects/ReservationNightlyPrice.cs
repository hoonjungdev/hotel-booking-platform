using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves the agreed price for one occupied night of a Reservation.</summary>
public sealed record ReservationNightlyPrice
{
    private ReservationNightlyPrice(DateOnly occupiedDate, Money price)
    {
        OccupiedDate = occupiedDate;
        Price = price;
    }

    /// <summary>Gets the occupied date covered by this agreed price.</summary>
    public DateOnly OccupiedDate { get; }

    /// <summary>Gets the immutable agreed price for the occupied date.</summary>
    public Money Price { get; }

    /// <summary>Creates one Booking-owned nightly price copied from an accepted Price Quote.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the occupied date or price is missing.
    /// </exception>
    public static ReservationNightlyPrice Create(DateOnly occupiedDate, Money price)
    {
        if (occupiedDate == default)
        {
            throw new DomainArgumentException(
                "Reservation nightly price occupied date is required.",
                nameof(occupiedDate));
        }

        if (price is null)
        {
            throw new DomainArgumentException(
                "Reservation nightly price is required.",
                nameof(price));
        }

        return new ReservationNightlyPrice(occupiedDate, price);
    }
}
