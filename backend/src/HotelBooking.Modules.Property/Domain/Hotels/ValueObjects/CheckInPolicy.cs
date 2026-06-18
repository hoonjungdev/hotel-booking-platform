namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

public sealed record CheckInPolicy(
    TimeOnly CheckInFrom,
    TimeOnly? CheckInUntil,
    TimeOnly CheckOutUntil,
    bool AllowsEarlyCheckIn,
    bool AllowsLateCheckOut);
