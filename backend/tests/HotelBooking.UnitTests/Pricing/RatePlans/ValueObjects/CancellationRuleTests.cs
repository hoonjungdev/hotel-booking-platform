using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.RatePlans.ValueObjects;

/// <summary>
/// Proves the valid boundary and penalty pairing for one cancellation window.
/// </summary>
public class CancellationRuleTests
{
    [Fact]
    public void Create_preserves_the_minimum_notice_and_penalty()
    {
        CancellationPenalty penalty = CancellationPenalty.PercentageOfTotal(50m);

        CancellationRule rule = new(TimeSpan.FromDays(3), penalty);

        Assert.Equal(TimeSpan.FromDays(3), rule.MinimumNotice);
        Assert.Equal(penalty, rule.Penalty);
    }

    [Fact]
    public void Create_rejects_negative_minimum_notice()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            new CancellationRule(TimeSpan.FromTicks(-1), CancellationPenalty.NoPenalty()));

        Assert.Equal("minimumNotice", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_missing_penalty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            new CancellationRule(TimeSpan.Zero, null!));

        Assert.Equal("penalty", exception.ParamName);
    }
}
