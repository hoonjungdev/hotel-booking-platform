using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.Reservations;

/// <summary>Identifies a Reservation lifecycle record owned by Booking.</summary>
public readonly record struct ReservationId
{
    /// <summary>Gets the underlying identifier value.</summary>
    public Guid Value { get; }

    private ReservationId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new DomainArgumentException("ReservationId cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>Creates a new Reservation identifier.</summary>
    public static ReservationId Create()
    {
        return new ReservationId(Guid.NewGuid());
    }

    /// <summary>Rehydrates a Reservation identifier from a persisted value.</summary>
    public static ReservationId From(Guid value)
    {
        return new ReservationId(value);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return Value.ToString();
    }
}
