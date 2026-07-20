using System.Collections.ObjectModel;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves the ordered Cancellation Policy accepted with a Reservation price.</summary>
public sealed class CancellationPolicySnapshot : IEquatable<CancellationPolicySnapshot>
{
    private readonly ReadOnlyCollection<CancellationRuleSnapshot> _rules;

    private CancellationPolicySnapshot(CancellationRuleSnapshot[] rules)
    {
        _rules = Array.AsReadOnly(rules);
    }

    /// <summary>Gets rules ordered from the longest minimum notice to the fallback boundary.</summary>
    public IReadOnlyList<CancellationRuleSnapshot> Rules => _rules;

    /// <summary>Creates a complete immutable copy of accepted cancellation terms.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when rules are missing, contain duplicate boundaries, or omit the zero-notice fallback.
    /// </exception>
    public static CancellationPolicySnapshot Create(params CancellationRuleSnapshot[] rules)
    {
        if (rules is null || rules.Length == 0)
        {
            throw new DomainArgumentException(
                "Cancellation policy snapshot requires at least one rule.",
                nameof(rules));
        }

        if (rules.Any(rule => rule is null))
        {
            throw new DomainArgumentException(
                "Cancellation policy snapshot rules cannot contain a missing rule.",
                nameof(rules));
        }

        if (!rules.Any(rule => rule.MinimumNotice == TimeSpan.Zero))
        {
            throw new DomainArgumentException(
                "Cancellation policy snapshot requires a zero-notice fallback rule.",
                nameof(rules));
        }

        if (rules
            .GroupBy(rule => rule.MinimumNotice)
            .Any(group => group.Count() > 1))
        {
            throw new DomainArgumentException(
                "Cancellation policy snapshot notice boundaries must be unique.",
                nameof(rules));
        }

        CancellationRuleSnapshot[] orderedRules = rules
            .OrderByDescending(rule => rule.MinimumNotice)
            .ToArray();

        return new CancellationPolicySnapshot(orderedRules);
    }

    /// <inheritdoc />
    public bool Equals(CancellationPolicySnapshot? other)
    {
        return ReferenceEquals(this, other)
            || other is not null && _rules.SequenceEqual(other._rules);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is CancellationPolicySnapshot other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (CancellationRuleSnapshot rule in _rules)
        {
            hash.Add(rule);
        }

        return hash.ToHashCode();
    }
}
