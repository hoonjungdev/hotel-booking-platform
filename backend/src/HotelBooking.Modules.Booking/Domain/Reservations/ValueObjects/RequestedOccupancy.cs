using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves the adult and child Guest composition agreed for one reserved room.</summary>
public sealed record RequestedOccupancy
{
    private RequestedOccupancy(int adults, int children)
    {
        Adults = adults;
        Children = children;
    }

    /// <summary>Gets the number of adult Guests included in the Reservation.</summary>
    public int Adults { get; }

    /// <summary>Gets the number of child Guests included in the Reservation.</summary>
    public int Children { get; }

    /// <summary>Creates a valid Requested Occupancy for one reserved room.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when there is no adult Guest or the child count is negative.
    /// </exception>
    public static RequestedOccupancy Create(int adults, int children)
    {
        if (adults <= 0)
        {
            throw new DomainArgumentException(
                "Requested occupancy requires at least one adult.",
                nameof(adults));
        }

        if (children < 0)
        {
            throw new DomainArgumentException(
                "Requested occupancy children cannot be negative.",
                nameof(children));
        }

        return new RequestedOccupancy(adults, children);
    }
}
