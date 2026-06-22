using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.RoomTypes.ValueObjects;

public class BedCompositionTests
{
    [Fact]
    public void Create_BedComposition_with_valid_data()
    {
        var bedComposition = BedComposition.Create(BedType.Double, 2);
        Assert.Equal(BedType.Double, bedComposition.BedType);
        Assert.Equal(2, bedComposition.Quantity);
    }

    [Theory]
    [InlineData((BedType)0)]
    [InlineData((BedType)999)]
    public void Create_throws_when_invalid_bed_type(BedType bedType)
    {
        Assert.Throws<DomainArgumentException>(() => BedComposition.Create(bedType, 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void Create_throws_when_quantity_is_less_than_1(int quantity)
    {
        Assert.Throws<DomainArgumentException>(() => BedComposition.Create(BedType.Single, quantity));
    }
}
