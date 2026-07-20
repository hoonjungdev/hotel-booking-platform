using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves one notice boundary and penalty from an accepted Cancellation Policy.</summary>
public sealed record CancellationRuleSnapshot
{
    /// <summary>Gets the minimum remaining time before check-in required for this rule.</summary>
    public TimeSpan MinimumNotice { get; }

    /// <summary>Gets the penalty agreed at this notice boundary.</summary>
    public CancellationPenaltySnapshot Penalty { get; }

    /// <summary>Creates one immutable rule in a Reservation's Cancellation Policy snapshot.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the notice is negative or the penalty is missing.
    /// </exception>
    public CancellationRuleSnapshot(
        TimeSpan minimumNotice,
        CancellationPenaltySnapshot penalty)
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
