namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Represents a hotel's geographic coordinates.</summary>
/// <param name="Latitude">The latitude coordinate.</param>
/// <param name="Longitude">The longitude coordinate.</param>
public sealed record GeoLocation(
    decimal Latitude,
    decimal Longitude)
{
    private GeoLocation()
        : this(default, default)
    {
    }
}
