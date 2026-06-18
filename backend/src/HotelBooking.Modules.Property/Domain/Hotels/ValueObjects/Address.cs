namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

public sealed record Address(
    string CountryCode,
    string Region,
    string City,
    string StreetAddress,
    string? DetailAddress,
    string? PostalCode);
