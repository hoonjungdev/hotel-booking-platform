using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.RatePlans.ValueObjects;

/// <summary>
/// Proves the valid forms of a cancellation penalty calculation basis.
/// </summary>
public class CancellationPenaltyTests
{
    [Fact]
    public void NoPenalty_has_no_percentage_or_night_count()
    {
        CancellationPenalty penalty = CancellationPenalty.NoPenalty();

        Assert.Equal(CancellationPenaltyType.None, penalty.Type);
        Assert.Null(penalty.Percentage);
        Assert.Null(penalty.Nights);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("100.01")]
    public void PercentageOfTotal_rejects_a_percentage_outside_the_chargeable_range(
        string percentageText)
    {
        decimal percentage = decimal.Parse(percentageText, System.Globalization.CultureInfo.InvariantCulture);

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPenalty.PercentageOfTotal(percentage));

        Assert.Equal("percentage", exception.ParamName);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("50")]
    [InlineData("100")]
    public void PercentageOfTotal_accepts_a_percentage_in_the_chargeable_range(
        string percentageText)
    {
        decimal percentage = decimal.Parse(percentageText, System.Globalization.CultureInfo.InvariantCulture);

        CancellationPenalty penalty = CancellationPenalty.PercentageOfTotal(percentage);

        Assert.Equal(CancellationPenaltyType.PercentageOfTotal, penalty.Type);
        Assert.Equal(percentage, penalty.Percentage);
        Assert.Null(penalty.Nights);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void NumberOfNights_rejects_a_non_positive_night_count(int nights)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            CancellationPenalty.NumberOfNights(nights));

        Assert.Equal("nights", exception.ParamName);
    }

    [Fact]
    public void NumberOfNights_preserves_the_penalized_night_count()
    {
        CancellationPenalty penalty = CancellationPenalty.NumberOfNights(2);

        Assert.Equal(CancellationPenaltyType.NumberOfNights, penalty.Type);
        Assert.Equal(2, penalty.Nights);
        Assert.Null(penalty.Percentage);
    }

    [Fact]
    public void FullStay_is_the_complete_stay_percentage()
    {
        CancellationPenalty penalty = CancellationPenalty.FullStay();

        Assert.Equal(CancellationPenaltyType.PercentageOfTotal, penalty.Type);
        Assert.Equal(100m, penalty.Percentage);
    }
}
