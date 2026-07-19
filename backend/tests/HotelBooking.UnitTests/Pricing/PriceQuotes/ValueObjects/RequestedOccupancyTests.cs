using HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Pricing.PriceQuotes.ValueObjects;

/// <summary>Proves the guest-count boundaries for one requested room.</summary>
public class RequestedOccupancyTests
{
    [Fact]
    public void Create_preserves_the_requested_guest_composition()
    {
        RequestedOccupancy occupancy = RequestedOccupancy.Create(2, 1);

        Assert.Equal(2, occupancy.Adults);
        Assert.Equal(1, occupancy.Children);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_rejects_a_request_without_an_adult(int adults)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RequestedOccupancy.Create(adults, 0));

        Assert.Equal("adults", exception.ParamName);
    }

    [Fact]
    public void Create_allows_a_request_without_children()
    {
        RequestedOccupancy occupancy = RequestedOccupancy.Create(1, 0);

        Assert.Equal(0, occupancy.Children);
    }

    [Fact]
    public void Create_rejects_a_negative_child_count()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            RequestedOccupancy.Create(1, -1));

        Assert.Equal("children", exception.ParamName);
    }

    [Fact]
    public void Equal_guest_compositions_have_value_equality()
    {
        RequestedOccupancy first = RequestedOccupancy.Create(2, 1);
        RequestedOccupancy second = RequestedOccupancy.Create(2, 1);

        Assert.Equal(first, second);
    }
}
