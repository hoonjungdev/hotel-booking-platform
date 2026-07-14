namespace HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;

/// <summary>Defines the supported physical bed categories in a room type.</summary>
public enum BedType
{
    /// <summary>A single bed.</summary>
    Single = 1,
    /// <summary>A double bed.</summary>
    Double = 2,
    /// <summary>A king-size bed.</summary>
    King = 3,
    /// <summary>A queen-size bed.</summary>
    Queen = 4,
    /// <summary>A twin bed.</summary>
    Twin = 5
}
