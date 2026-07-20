using HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Booking.Reservations.ValueObjects;

/// <summary>Proves completeness, normalization, and penalty rules for snapshotted cancellation terms.</summary>
public class CancellationPolicySnapshotTests
{
    [Fact]
    public void Create_orders_complete_rules_by_descending_notice()
    {
        CancellationRuleSnapshot fallback = new(
            TimeSpan.Zero,
            CancellationPenaltySnapshot.FullStay());
        CancellationRuleSnapshot freeCancellation = new(
            TimeSpan.FromDays(3),
            CancellationPenaltySnapshot.NoPenalty());

        CancellationPolicySnapshot policy = CancellationPolicySnapshot.Create(
            fallback,
            freeCancellation);

        Assert.Collection(
            policy.Rules,
            rule => Assert.Equal(TimeSpan.FromDays(3), rule.MinimumNotice),
            rule => Assert.Equal(TimeSpan.Zero, rule.MinimumNotice));
    }

    [Fact]
    public void Create_copies_the_rule_collection()
    {
        CancellationRuleSnapshot fallback = new(
            TimeSpan.Zero,
            CancellationPenaltySnapshot.FullStay());
        CancellationRuleSnapshot[] rules = [fallback];

        CancellationPolicySnapshot policy = CancellationPolicySnapshot.Create(rules);
        rules[0] = new CancellationRuleSnapshot(
            TimeSpan.Zero,
            CancellationPenaltySnapshot.NoPenalty());

        Assert.Equal(CancellationPenaltyType.PercentageOfTotal, policy.Rules[0].Penalty.Type);
    }

    [Fact]
    public void Equivalent_policies_have_structural_value_equality_after_normalization()
    {
        CancellationPolicySnapshot first = CancellationPolicySnapshot.Create(
            new CancellationRuleSnapshot(
                TimeSpan.Zero,
                CancellationPenaltySnapshot.FullStay()),
            new CancellationRuleSnapshot(
                TimeSpan.FromDays(3),
                CancellationPenaltySnapshot.NoPenalty()));
        CancellationPolicySnapshot second = CancellationPolicySnapshot.Create(
            new CancellationRuleSnapshot(
                TimeSpan.FromDays(3),
                CancellationPenaltySnapshot.NoPenalty()),
            new CancellationRuleSnapshot(
                TimeSpan.Zero,
                CancellationPenaltySnapshot.FullStay()));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Policies_with_different_penalties_are_not_equal()
    {
        CancellationPolicySnapshot free = CancellationPolicySnapshot.Create(
            new CancellationRuleSnapshot(
                TimeSpan.Zero,
                CancellationPenaltySnapshot.NoPenalty()));
        CancellationPolicySnapshot nonRefundable = CancellationPolicySnapshot.Create(
            new CancellationRuleSnapshot(
                TimeSpan.Zero,
                CancellationPenaltySnapshot.FullStay()));

        Assert.NotEqual(free, nonRefundable);
    }

    [Fact]
    public void Create_rejects_a_missing_rule_collection()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicySnapshot.Create(null!));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_an_empty_rule_collection()
    {
        Assert.Throws<DomainArgumentException>(() => CancellationPolicySnapshot.Create([]));
    }

    [Fact]
    public void Create_rejects_a_missing_rule()
    {
        Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicySnapshot.Create([null!]));
    }

    [Fact]
    public void Create_rejects_a_policy_without_a_zero_notice_fallback()
    {
        CancellationRuleSnapshot rule = new(
            TimeSpan.FromDays(1),
            CancellationPenaltySnapshot.NoPenalty());

        Assert.Throws<DomainArgumentException>(() => CancellationPolicySnapshot.Create(rule));
    }

    [Fact]
    public void Create_rejects_duplicate_notice_boundaries()
    {
        CancellationRuleSnapshot first = new(
            TimeSpan.Zero,
            CancellationPenaltySnapshot.NoPenalty());
        CancellationRuleSnapshot second = new(
            TimeSpan.Zero,
            CancellationPenaltySnapshot.FullStay());

        Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicySnapshot.Create(first, second));
    }

    [Fact]
    public void Cancellation_rule_rejects_negative_notice()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            new CancellationRuleSnapshot(
                TimeSpan.FromTicks(-1),
                CancellationPenaltySnapshot.NoPenalty()));

        Assert.Equal("minimumNotice", exception.ParamName);
    }

    [Fact]
    public void Cancellation_rule_rejects_a_missing_penalty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            new CancellationRuleSnapshot(TimeSpan.Zero, null!));

        Assert.Equal("penalty", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Percentage_penalty_rejects_an_out_of_range_value(decimal percentage)
    {
        Assert.Throws<DomainArgumentException>(() =>
            CancellationPenaltySnapshot.PercentageOfTotal(percentage));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Night_penalty_rejects_a_non_positive_night_count(int nights)
    {
        Assert.Throws<DomainArgumentException>(() =>
            CancellationPenaltySnapshot.NumberOfNights(nights));
    }

    [Fact]
    public void Penalty_factories_preserve_their_calculation_basis()
    {
        CancellationPenaltySnapshot noPenalty = CancellationPenaltySnapshot.NoPenalty();
        CancellationPenaltySnapshot percentage =
            CancellationPenaltySnapshot.PercentageOfTotal(25m);
        CancellationPenaltySnapshot nights = CancellationPenaltySnapshot.NumberOfNights(2);

        Assert.Equal(CancellationPenaltyType.None, noPenalty.Type);
        Assert.Null(noPenalty.Percentage);
        Assert.Null(noPenalty.Nights);
        Assert.Equal(CancellationPenaltyType.PercentageOfTotal, percentage.Type);
        Assert.Equal(25m, percentage.Percentage);
        Assert.Equal(CancellationPenaltyType.NumberOfNights, nights.Type);
        Assert.Equal(2, nights.Nights);
    }
}
