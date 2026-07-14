namespace HotelBooking.Modules.Property.Domain.RoomTypes;

/// <summary>Describes whether a room type can be offered to guests.</summary>
public enum RoomTypeStatus
{
    /// <summary>The room type is being configured.</summary>
    Draft = 1,
    /// <summary>The room type is available for sale.</summary>
    Active = 2,
    /// <summary>The room type is temporarily unavailable for sale.</summary>
    Suspended = 3,
    /// <summary>The room type is permanently closed.</summary>
    Closed = 4
}
