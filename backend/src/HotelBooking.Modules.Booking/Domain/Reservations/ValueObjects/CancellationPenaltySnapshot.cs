using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves how an accepted Cancellation Penalty will be calculated.</summary>
public sealed record CancellationPenaltySnapshot
{
    private CancellationPenaltySnapshot(
        CancellationPenaltyType type,
        decimal? percentage = null,
        int? nights = null)
    {
        Type = type;
        Percentage = percentage;
        Nights = nights;
    }

    /// <summary>Gets the basis used to calculate the cancellation charge.</summary>
    public CancellationPenaltyType Type { get; }

    /// <summary>Gets the agreed total-price percentage when percentage pricing applies.</summary>
    public decimal? Percentage { get; }

    /// <summary>Gets the agreed number of occupied nights when nightly pricing applies.</summary>
    public int? Nights { get; }

    /// <summary>Creates a penalty-free cancellation term.</summary>
    public static CancellationPenaltySnapshot NoPenalty()
    {
        return new CancellationPenaltySnapshot(CancellationPenaltyType.None);
    }

    /// <summary>Creates a penalty calculated as a percentage of the agreed total price.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the percentage is not greater than zero and at most 100.
    /// </exception>
    public static CancellationPenaltySnapshot PercentageOfTotal(decimal percentage)
    {
        if (percentage <= 0m || percentage > 100m)
        {
            throw new DomainArgumentException(
                "Cancellation penalty percentage must be greater than zero and at most 100.",
                nameof(percentage));
        }

        return new CancellationPenaltySnapshot(
            CancellationPenaltyType.PercentageOfTotal,
            percentage: percentage);
    }

    /// <summary>Creates a penalty calculated from a positive number of agreed nightly prices.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the number of nights is not positive.
    /// </exception>
    public static CancellationPenaltySnapshot NumberOfNights(int nights)
    {
        if (nights <= 0)
        {
            throw new DomainArgumentException(
                "Cancellation penalty nights must be greater than zero.",
                nameof(nights));
        }

        return new CancellationPenaltySnapshot(
            CancellationPenaltyType.NumberOfNights,
            nights: nights);
    }

    /// <summary>Creates a penalty for the complete agreed stay price.</summary>
    public static CancellationPenaltySnapshot FullStay()
    {
        return PercentageOfTotal(100m);
    }
}
