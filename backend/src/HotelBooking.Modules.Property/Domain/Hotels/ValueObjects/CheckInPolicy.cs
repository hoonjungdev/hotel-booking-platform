namespace HotelBooking.Modules.Property.Domain.Hotels.ValueObjects;

/// <summary>Describes a hotel's guest arrival and departure time rules.</summary>
/// <param name="CheckInFrom">The earliest standard check-in time.</param>
/// <param name="CheckInUntil">The optional latest standard check-in time.</param>
/// <param name="CheckOutUntil">The latest standard check-out time.</param>
/// <param name="AllowsEarlyCheckIn">Whether early check-in may be offered.</param>
/// <param name="AllowsLateCheckOut">Whether late check-out may be offered.</param>
public sealed record CheckInPolicy(
    TimeOnly CheckInFrom,
    TimeOnly? CheckInUntil,
    TimeOnly CheckOutUntil,
    bool AllowsEarlyCheckIn,
    bool AllowsLateCheckOut)
{
    private CheckInPolicy()
        : this(default, null, default, false, false)
    {
    }
}
