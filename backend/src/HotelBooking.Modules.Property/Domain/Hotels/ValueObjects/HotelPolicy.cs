namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Describes guest eligibility, amenity, and deposit rules for a hotel.</summary>
/// <param name="AllowsSmoking">Whether smoking is permitted.</param>
/// <param name="AllowsPets">Whether pets are permitted.</param>
/// <param name="AllowsChildren">Whether children are permitted.</param>
/// <param name="MinimumCheckInAge">The optional minimum guest check-in age.</param>
/// <param name="RequiresDeposit">Whether a stay requires a deposit.</param>
public sealed record HotelPolicy(
    bool AllowsSmoking,
    bool AllowsPets,
    bool AllowsChildren,
    int? MinimumCheckInAge,
    bool RequiresDeposit);
