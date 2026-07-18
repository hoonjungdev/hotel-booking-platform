namespace HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;

/// <summary>
/// Identifies the price basis used to calculate a cancellation penalty.
/// </summary>
public enum CancellationPenaltyType
{
    /// <summary>No cancellation penalty applies.</summary>
    None = 1,

    /// <summary>The penalty is a percentage of the reservation's agreed total price.</summary>
    PercentageOfTotal = 2,

    /// <summary>The penalty is the agreed price of a specified number of occupied nights.</summary>
    NumberOfNights = 3
}
