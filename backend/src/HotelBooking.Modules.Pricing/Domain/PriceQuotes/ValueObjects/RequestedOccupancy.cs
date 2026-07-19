using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;

/// <summary>Represents the adult and child guest composition requested for one room.</summary>
public sealed record RequestedOccupancy
{
    private RequestedOccupancy(int adults, int children)
    {
        Adults = adults;
        Children = children;
    }

    /// <summary>Gets the number of adult guests requested for the room.</summary>
    public int Adults { get; }

    /// <summary>Gets the number of child guests requested for the room.</summary>
    public int Children { get; }

    /// <summary>Creates a valid guest composition for one requested room.</summary>
    /// <param name="adults">The positive number of adult guests.</param>
    /// <param name="children">The non-negative number of child guests.</param>
    /// <returns>The requested occupancy.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when there is no adult guest or the child count is negative.
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
