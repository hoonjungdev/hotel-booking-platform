using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.RatePlans.ValueObjects;

/// <summary>
/// Proves how a cancellation policy selects the agreed penalty from tiered notice periods.
/// </summary>
public class CancellationPolicyTests
{
    private static readonly DateTimeOffset CheckInAt =
        new(2026, 7, 10, 15, 0, 0, TimeSpan.FromHours(9));

    [Fact]
    public void DeterminePenalty_selects_no_penalty_before_the_seven_day_deadline()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(8)));

        Assert.Equal(CancellationPenalty.NoPenalty(), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_no_penalty_at_the_exact_seven_day_deadline()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(7)));

        Assert.Equal(CancellationPenalty.NoPenalty(), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_fifty_percent_immediately_after_the_seven_day_deadline()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(7)).AddTicks(1));

        Assert.Equal(CancellationPenalty.PercentageOfTotal(50m), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_fifty_percent_at_the_exact_three_day_deadline()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(3)));

        Assert.Equal(CancellationPenalty.PercentageOfTotal(50m), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_one_night_immediately_after_the_three_day_deadline()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(3)).AddTicks(1));

        Assert.Equal(CancellationPenalty.NumberOfNights(1), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_the_zero_notice_fallback_immediately_before_check_in()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.AddTicks(-1));

        Assert.Equal(CancellationPenalty.NumberOfNights(1), penalty);
    }

    [Fact]
    public void DeterminePenalty_selects_full_stay_for_a_non_refundable_policy()
    {
        CancellationPolicy policy = CancellationPolicy.Create(
            new CancellationRule(TimeSpan.Zero, CancellationPenalty.FullStay()));

        CancellationPenalty penalty = policy.DeterminePenalty(
            CheckInAt,
            CheckInAt.Subtract(TimeSpan.FromDays(30)));

        Assert.Equal(CancellationPenalty.FullStay(), penalty);
    }

    [Fact]
    public void Create_orders_rules_from_longest_to_shortest_notice()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        Assert.Collection(
            policy.Rules,
            rule => Assert.Equal(TimeSpan.FromDays(7), rule.MinimumNotice),
            rule => Assert.Equal(TimeSpan.FromDays(3), rule.MinimumNotice),
            rule => Assert.Equal(TimeSpan.Zero, rule.MinimumNotice));
    }

    [Fact]
    public void Equivalent_rule_schedules_are_equal_regardless_of_input_order()
    {
        CancellationPolicy first = CreateTieredPolicy();
        CancellationPolicy second = CancellationPolicy.Create(
            new CancellationRule(TimeSpan.FromDays(3), CancellationPenalty.PercentageOfTotal(50m)),
            new CancellationRule(TimeSpan.FromDays(7), CancellationPenalty.NoPenalty()),
            new CancellationRule(TimeSpan.Zero, CancellationPenalty.NumberOfNights(1)));

        Assert.Equal(first, second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Create_rejects_an_empty_rule_schedule()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicy.Create([]));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_rule_schedule()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicy.Create(null!));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_rule_in_the_schedule()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicy.Create(
                new CancellationRule(TimeSpan.Zero, CancellationPenalty.NoPenalty()),
                null!));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_schedule_without_a_zero_notice_fallback()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicy.Create(
                new CancellationRule(
                    TimeSpan.FromDays(7),
                    CancellationPenalty.NoPenalty())));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_duplicate_notice_boundaries()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPolicy.Create(
                new CancellationRule(TimeSpan.Zero, CancellationPenalty.NumberOfNights(1)),
                new CancellationRule(TimeSpan.FromDays(3), CancellationPenalty.NoPenalty()),
                new CancellationRule(TimeSpan.FromDays(3), CancellationPenalty.PercentageOfTotal(50m))));

        Assert.Equal("rules", exception.ParamName);
    }

    [Fact]
    public void DeterminePenalty_rejects_missing_check_in_time()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            policy.DeterminePenalty(default, CheckInAt.Subtract(TimeSpan.FromDays(7))));

        Assert.Equal("checkInAt", exception.ParamName);
    }

    [Fact]
    public void DeterminePenalty_rejects_missing_cancellation_time()
    {
        CancellationPolicy policy = CreateTieredPolicy();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            policy.DeterminePenalty(CheckInAt, default));

        Assert.Equal("cancelledAt", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void DeterminePenalty_rejects_cancellation_at_or_after_check_in(long ticksAfterCheckIn)
    {
        CancellationPolicy policy = CreateTieredPolicy();

        Assert.Throws<DomainException>(() =>
            policy.DeterminePenalty(CheckInAt, CheckInAt.AddTicks(ticksAfterCheckIn)));
    }

    private static CancellationPolicy CreateTieredPolicy()
    {
        return CancellationPolicy.Create(
            new CancellationRule(TimeSpan.Zero, CancellationPenalty.NumberOfNights(1)),
            new CancellationRule(TimeSpan.FromDays(7), CancellationPenalty.NoPenalty()),
            new CancellationRule(TimeSpan.FromDays(3), CancellationPenalty.PercentageOfTotal(50m)));
    }
}
