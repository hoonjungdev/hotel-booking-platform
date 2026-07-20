namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Identifies the agreed basis for a Reservation cancellation penalty.</summary>
public enum CancellationPenaltyType
{
    /// <summary>No cancellation penalty applies.</summary>
    None = 1,

    /// <summary>The penalty is a percentage of the agreed total price.</summary>
    PercentageOfTotal = 2,

    /// <summary>The penalty is the agreed price of a specified number of occupied nights.</summary>
    NumberOfNights = 3
}
