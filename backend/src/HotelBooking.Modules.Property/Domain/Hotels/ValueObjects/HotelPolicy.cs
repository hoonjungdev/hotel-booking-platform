namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

public sealed record HotelPolicy(
    bool AllowsSmoking,
    bool AllowsPets,
    bool AllowsChildren,
    int? MinimumCheckInAge,
    bool RequiresDeposit);
