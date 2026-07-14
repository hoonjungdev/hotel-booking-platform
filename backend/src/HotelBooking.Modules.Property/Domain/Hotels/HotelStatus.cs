namespace HotelBooking.Modules.Property.Domain.Hotels;

/// <summary>Describes the setup and operational lifecycle of a hotel.</summary>
public enum HotelStatus
{
    /// <summary>The hotel is being configured.</summary>
    Draft = 1,
    /// <summary>The hotel is complete and awaiting publication review.</summary>
    PendingReview = 2,
    /// <summary>The hotel is published and operational.</summary>
    Active = 3,
    /// <summary>The hotel is temporarily unavailable for operation.</summary>
    Suspended = 4,
    /// <summary>The hotel is permanently closed.</summary>
    Closed = 5
}
