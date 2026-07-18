using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;

/// <summary>
/// Associates a minimum notice period before check-in with its cancellation penalty term.
/// </summary>
public sealed record CancellationRule
{
    /// <summary>
    /// Gets the minimum remaining time before check-in required for this rule to apply.
    /// </summary>
    public TimeSpan MinimumNotice { get; }

    /// <summary>Gets the penalty term selected when the minimum notice is satisfied.</summary>
    public CancellationPenalty Penalty { get; }

    /// <summary>Creates one boundary in a cancellation policy's penalty schedule.</summary>
    /// <param name="minimumNotice">
    /// The non-negative remaining time before check-in at which this rule begins to apply.
    /// </param>
    /// <param name="penalty">The penalty term for cancellations within this boundary.</param>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the notice is negative or the penalty is missing.
    /// </exception>
    public CancellationRule(TimeSpan minimumNotice, CancellationPenalty penalty)
    {
        if (minimumNotice < TimeSpan.Zero)
        {
            throw new DomainArgumentException(
                "Cancellation rule minimum notice cannot be negative.",
                nameof(minimumNotice));
        }

        MinimumNotice = minimumNotice;
        Penalty = penalty
            ?? throw new DomainArgumentException(
                "Cancellation rule penalty is required.",
                nameof(penalty));
    }
}
