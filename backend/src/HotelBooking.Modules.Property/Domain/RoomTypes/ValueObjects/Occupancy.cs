using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

public sealed record Occupancy
{
    public int MaxAdults { get; }
    public int MaxChildren { get; }
    public int MaxOccupancy { get; }

    private Occupancy(int maxAdults, int maxChildren, int maxOccupancy)
    {
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        MaxOccupancy = maxOccupancy;
    }

    public static Occupancy Create(int maxAdults, int maxChildren, int maxOccupancy)
    {
        if (maxAdults < 1)
        {
            throw new DomainArgumentException("Max adults cannot be less than 1.", nameof(maxAdults));
        }
        if (maxChildren < 0)
        {
            throw new DomainArgumentException("Max children cannot be negative.", nameof(maxChildren));
        }
        if (maxOccupancy < 1)
        {
            throw new DomainArgumentException("Max occupancy cannot be less than 1.", nameof(maxOccupancy));
        }
        if (maxAdults > maxOccupancy)
        {
            throw new DomainArgumentException("Max adults cannot be greater than max occupancy.", nameof(maxAdults));
        }
        if (maxChildren > maxOccupancy)
        {
            throw new DomainArgumentException("Max children cannot be greater than max occupancy.", nameof(maxChildren));
        }
        if (maxOccupancy > maxAdults + maxChildren)
        {
            throw new DomainArgumentException("Max occupancy cannot exceed sum of max adults and max children.", nameof(maxOccupancy));
        }

        return new Occupancy(maxAdults, maxChildren, maxOccupancy);
    }
}
