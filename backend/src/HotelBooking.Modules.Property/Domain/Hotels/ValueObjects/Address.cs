namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Represents the postal location of a hotel.</summary>
/// <param name="CountryCode">The ISO country code.</param>
/// <param name="Region">The state, province, or region.</param>
/// <param name="City">The city or locality.</param>
/// <param name="StreetAddress">The primary street address.</param>
/// <param name="DetailAddress">Optional additional address details.</param>
/// <param name="PostalCode">The optional postal code.</param>
public sealed record Address(
    string CountryCode,
    string Region,
    string City,
    string StreetAddress,
    string? DetailAddress,
    string? PostalCode);
