using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.RoomTypes.ValueObjects;

public class OccupancyTests
{
    [Fact]
    public void Create_returns_occupancy_with_valid_data()
    {
        var occupancy = Occupancy.Create(2, 1, 2);

        Assert.Equal(2, occupancy.MaxAdults);
        Assert.Equal(1, occupancy.MaxChildren);
        Assert.Equal(2, occupancy.MaxOccupancy);
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(0, 1, 1)]
    public void Create_throws_when_max_adults_less_than_1(int maxAdults, int maxChildren, int maxOccupancy)
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(maxAdults, maxChildren, maxOccupancy));
    }

    [Fact]
    public void Create_throws_when_max_children_is_negative()
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(1, -1, 1));
    }

    [Theory]
    [InlineData(1, 1, -1)]
    [InlineData(1, 1, 0)]
    public void Create_throws_when_max_occupancy_less_than_1(int maxAdults, int maxChildren, int maxOccupancy)
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(maxAdults, maxChildren, maxOccupancy));
    }

    [Theory]
    [InlineData(1, 1, 3)]
    [InlineData(2, 0, 3)]
    public void Create_throws_when_greater_then_max_occupancy(int maxAdults, int maxChildren, int maxOccupancy)
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(maxAdults, maxChildren, maxOccupancy));
    }

    [Fact]
    public void Create_throws_when_max_adults_greater_than_max_occupancy()
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(3, 1, 2));
    }

    [Fact]
    public void Create_throws_when_max_children_greater_than_max_occupancy()
    {
        Assert.Throws<DomainArgumentException>(() => Occupancy.Create(1, 3, 2));
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void CanAccommodate_returns_true_when_guest_occupancy_fits_room_type(
        int adults,
        int children)
    {
        var occupancy = Occupancy.Create(2, 2, 4);

        Assert.True(occupancy.CanAccommodate(adults, children));
    }
}
