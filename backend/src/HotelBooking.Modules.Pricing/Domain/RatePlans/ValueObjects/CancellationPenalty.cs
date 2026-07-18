using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;

/// <summary>
/// Describes how a cancellation charge is calculated without calculating its monetary amount.
/// </summary>
public sealed record CancellationPenalty
{
    /// <summary>Gets the basis used to calculate the cancellation charge.</summary>
    public CancellationPenaltyType Type { get; }

    /// <summary>Gets the percentage of the agreed total price when that basis applies.</summary>
    public decimal? Percentage { get; }

    /// <summary>Gets the number of occupied nights charged when that basis applies.</summary>
    public int? Nights { get; }

    private CancellationPenalty(
        CancellationPenaltyType type,
        decimal? percentage = null,
        int? nights = null)
    {
        Type = type;
        Percentage = percentage;
        Nights = nights;
    }

    /// <summary>Creates a penalty term under which cancellation is free of charge.</summary>
    /// <returns>A cancellation term with no penalty calculation basis.</returns>
    public static CancellationPenalty NoPenalty()
    {
        return new CancellationPenalty(CancellationPenaltyType.None);
    }

    /// <summary>Creates a penalty calculated as a percentage of the agreed total price.</summary>
    /// <param name="percentage">The chargeable percentage, greater than zero and at most 100.</param>
    /// <returns>A percentage-based cancellation penalty.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when <paramref name="percentage"/> is outside the chargeable range.
    /// </exception>
    public static CancellationPenalty PercentageOfTotal(decimal percentage)
    {
        if (percentage <= 0m || percentage > 100m)
        {
            throw new DomainArgumentException(
                "Cancellation penalty percentage must be greater than zero and at most 100.",
                nameof(percentage));
        }

        return new CancellationPenalty(
            CancellationPenaltyType.PercentageOfTotal,
            percentage: percentage);
    }

    /// <summary>Creates a penalty calculated from the agreed prices of occupied nights.</summary>
    /// <param name="nights">The positive number of occupied nights to charge.</param>
    /// <returns>A night-based cancellation penalty.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when <paramref name="nights"/> is not positive.
    /// </exception>
    public static CancellationPenalty NumberOfNights(int nights)
    {
        if (nights <= 0)
        {
            throw new DomainArgumentException(
                "Cancellation penalty nights must be greater than zero.",
                nameof(nights));
        }

        return new CancellationPenalty(
            CancellationPenaltyType.NumberOfNights,
            nights: nights);
    }

    /// <summary>Creates a penalty for the reservation's complete agreed total price.</summary>
    /// <returns>A 100-percent penalty of the agreed total price.</returns>
    public static CancellationPenalty FullStay()
    {
        return PercentageOfTotal(100m);
    }
}
