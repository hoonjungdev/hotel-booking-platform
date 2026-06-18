namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

public sealed record ContactInfo(
    string? Phone,
    string? Email,
    string? Website);
