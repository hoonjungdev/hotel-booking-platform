using HotelBooking.Modules.Property.Domain.Hotels;
using HotelBooking.Modules.Property.Domain.RoomTypes;
using HotelBooking.Modules.Property.Domain.RoomTypes.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.UnitTests.Property.RoomTypes;

public class RoomTypeTests
{
    [Fact]
    public void CreateDraft_creates_room_type_in_draft_status()
    {
        var id = RoomTypeId.Create();
        var hotelId = HotelId.Create();
        var name = " Standard Room  ";
        var code = RoomTypeCode.Create("STD");
        var occupancy = Occupancy.Create(2, 1, 2);
        var bedCompositions = new List<BedComposition> { BedComposition.Create(BedType.Single, 1) };
        DateTimeOffset createdAt = new(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);

        var roomType = RoomType.CreateDraft(
            id,
            hotelId,
            name,
            code,
            occupancy,
            bedCompositions,
            createdAt);

        Assert.Equal(id, roomType.Id);
        Assert.Equal(hotelId, roomType.HotelId);
        Assert.Equal("Standard Room", roomType.Name);
        Assert.Equal(code, roomType.Code);
        Assert.Equal(RoomTypeStatus.Draft, roomType.Status);
        Assert.Equal(occupancy, roomType.Occupancy);
        Assert.Collection(
            roomType.BedCompositions,
            bed =>
            {
                Assert.Equal(BedType.Single, bed.BedType);
                Assert.Equal(1, bed.Quantity);
            });
        Assert.Equal(createdAt, roomType.CreatedAt);
    }

    [Fact]
    public void CreateDraft_throws_when_id_is_default()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            default,
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("id", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_hotel_id_is_default()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            default,
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("hotelId", domainArgumentException.ParamName);
    }

    [Theory]
    [InlineData("  ")]
    [InlineData(null)]
    public void CreateDraft_throws_when_name_is_missing(string? name)
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            name!,
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("name", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_name_is_longer_than_100_characters()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            new string('a', 101),
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("name", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_code_is_null()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            null!,
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("code", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_occupancy_is_null()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            null!,
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("occupancy", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_bed_compositions_are_empty()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("bedCompositions", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_bed_compositions_is_null()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            null!,
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("bedCompositions", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_throws_when_bed_compositions_contain_null()
    {
        DomainArgumentException domainArgumentException = Assert.Throws<DomainArgumentException>(() => RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [null!],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero)));

        Assert.Equal("bedCompositions", domainArgumentException.ParamName);
    }

    [Fact]
    public void CreateDraft_defensively_copies_bed_compositions()
    {
        var bedCompositions = new List<BedComposition> { BedComposition.Create(BedType.Single, 1) };
        var roomType = RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            bedCompositions,
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero));

        bedCompositions.Clear();

        Assert.Single(roomType.BedCompositions);
        Assert.Equal(BedType.Single, roomType.BedCompositions[0].BedType);
        Assert.Equal(1, roomType.BedCompositions[0].Quantity);
    }
}
