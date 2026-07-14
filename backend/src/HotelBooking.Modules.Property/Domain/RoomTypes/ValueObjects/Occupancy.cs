using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

/// <summary>Defines the adult, child, and combined guest limits of a room type.</summary>
public sealed record Occupancy
{
    /// <summary>Gets the maximum adult guest count.</summary>
    public int MaxAdults { get; }
    /// <summary>Gets the maximum child guest count.</summary>
    public int MaxChildren { get; }
    /// <summary>Gets the maximum combined guest count.</summary>
    public int MaxOccupancy { get; }

    private Occupancy(int maxAdults, int maxChildren, int maxOccupancy)
    {
        MaxAdults = maxAdults;
        MaxChildren = maxChildren;
        MaxOccupancy = maxOccupancy;
    }

    /// <summary>Creates internally consistent room type occupancy limits.</summary>
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

    /// <summary>Determines whether the requested guest composition fits all configured limits.</summary>
    public bool CanAccommodate(int adults, int children)
    {
        return adults <= MaxAdults
            && adults > 0
            && children <= MaxChildren
            && children >= 0
            && adults + children <= MaxOccupancy;
    }
}
