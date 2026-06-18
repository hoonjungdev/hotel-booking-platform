using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.SharedKernel.ValueObjects;

public class StayDateRangeTests
{
    [Fact]
    public void Calculates_nights_with_check_in_included_check_out_excluded()
    {
        var stayDateRange = new StayDateRange(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 3));

        Assert.Equal(2, stayDateRange.Nights);
    }

    [Fact]
    public void Throws_when_check_in_is_after_check_out()
    {
        DomainException exception = Assert.Throws<DomainException>(() =>
            new StayDateRange(
                new DateOnly(2026, 7, 3),
                new DateOnly(2026, 7, 1)));

        Assert.Equal("Check-in date must be before check-out date", exception.Message);
    }

    [Fact]
    public void Throws_when_check_in_is_the_same_as_check_out()
    {
        Assert.Throws<DomainException>(() =>
            new StayDateRange(
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 1)));
    }

    [Fact]
    public void Returns_occupied_dates_with_check_in_included_and_check_out_excluded()
    {
        var stayDateRange = new StayDateRange(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 4));

        Assert.Equal(
            [
                new DateOnly(2026, 7, 1),
                new DateOnly(2026, 7, 2),
                new DateOnly(2026, 7, 3)
            ],
            stayDateRange.OccupiedDates);
    }
}
