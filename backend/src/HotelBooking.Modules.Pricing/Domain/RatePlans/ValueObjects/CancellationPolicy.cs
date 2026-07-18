using System.Collections.ObjectModel;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;

/// <summary>
/// Represents the ordered penalty schedule agreed for cancelling a confirmed reservation.
/// </summary>
public sealed class CancellationPolicy : IEquatable<CancellationPolicy>
{
    private readonly ReadOnlyCollection<CancellationRule> _rules;

    private CancellationPolicy(CancellationRule[] rules)
    {
        _rules = Array.AsReadOnly(rules);
    }

    /// <summary>
    /// Gets policy rules ordered from the longest minimum notice to the zero-notice fallback.
    /// </summary>
    public IReadOnlyList<CancellationRule> Rules => _rules;

    /// <summary>Creates a complete cancellation penalty schedule.</summary>
    /// <param name="rules">
    /// Rules with unique notice boundaries, including a zero-notice fallback for every
    /// pre-check-in cancellation.
    /// </param>
    /// <returns>A normalized policy ordered by minimum notice.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when rules are missing, contain duplicate boundaries, or omit the zero-notice fallback.
    /// </exception>
    public static CancellationPolicy Create(params CancellationRule[] rules)
    {
        if (rules is null || rules.Length == 0)
        {
            throw new DomainArgumentException(
                "Cancellation policy requires at least one rule.",
                nameof(rules));
        }

        if (rules.Any(rule => rule is null))
        {
            throw new DomainArgumentException(
                "Cancellation policy rules cannot contain a missing rule.",
                nameof(rules));
        }

        if (!rules.Any(rule => rule.MinimumNotice == TimeSpan.Zero))
        {
            throw new DomainArgumentException(
                "Cancellation policy requires a zero-notice fallback rule.",
                nameof(rules));
        }

        if (rules
            .GroupBy(rule => rule.MinimumNotice)
            .Any(group => group.Count() > 1))
        {
            throw new DomainArgumentException(
                "Cancellation policy notice boundaries must be unique.",
                nameof(rules));
        }

        CancellationRule[] orderedRules = rules
            .OrderByDescending(rule => rule.MinimumNotice)
            .ToArray();

        return new CancellationPolicy(orderedRules);
    }

    /// <summary>
    /// Selects the penalty term agreed for an explicit cancellation time before check-in.
    /// </summary>
    /// <param name="checkInAt">The hotel's effective check-in timestamp for the reservation.</param>
    /// <param name="cancelledAt">The explicit timestamp at which cancellation occurs.</param>
    /// <returns>
    /// The rule's penalty term; monetary calculation remains based on the reservation price snapshot.
    /// </returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when either timestamp is missing.
    /// </exception>
    /// <exception cref="DomainException">
    /// Thrown when the cancellation is evaluated at or after check-in.
    /// </exception>
    public CancellationPenalty DeterminePenalty(
        DateTimeOffset checkInAt,
        DateTimeOffset cancelledAt)
    {
        if (checkInAt == default)
        {
            throw new DomainArgumentException("Check-in time is required.", nameof(checkInAt));
        }

        if (cancelledAt == default)
        {
            throw new DomainArgumentException("Cancellation time is required.", nameof(cancelledAt));
        }

        if (cancelledAt >= checkInAt)
        {
            throw new DomainException(
                "Cancellation penalty must be determined before check-in.");
        }

        TimeSpan remainingNotice = checkInAt - cancelledAt;

        foreach (CancellationRule rule in _rules)
        {
            if (remainingNotice >= rule.MinimumNotice)
            {
                return rule.Penalty;
            }
        }

        throw new InvalidOperationException(
            "A valid cancellation policy must contain a zero-notice fallback rule.");
    }

    /// <inheritdoc />
    public bool Equals(CancellationPolicy? other)
    {
        return ReferenceEquals(this, other)
            || other is not null && _rules.SequenceEqual(other._rules);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CancellationPolicy other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (CancellationRule rule in _rules)
        {
            hash.Add(rule);
        }

        return hash.ToHashCode();
    }
}
