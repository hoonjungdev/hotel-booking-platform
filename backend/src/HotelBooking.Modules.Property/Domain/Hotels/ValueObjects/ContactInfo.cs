namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Provides guest-facing contact channels for a hotel.</summary>
/// <param name="Phone">The optional contact phone number.</param>
/// <param name="Email">The optional contact email address.</param>
/// <param name="Website">The optional hotel website.</param>
public sealed record ContactInfo(
    string? Phone,
    string? Email,
    string? Website);
