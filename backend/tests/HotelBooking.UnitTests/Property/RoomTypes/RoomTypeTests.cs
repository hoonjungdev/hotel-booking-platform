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

    [Fact]
    public void Activate_changes_draft_room_type_to_active()
    {
        RoomType roomType = CreateDraftRoomType();

        roomType.Activate();

        Assert.Equal(RoomTypeStatus.Active, roomType.Status);
    }

    [Theory]
    [InlineData(RoomTypeStatus.Active)]
    [InlineData(RoomTypeStatus.Suspended)]
    [InlineData(RoomTypeStatus.Closed)]
    public void Activate_throws_exception_if_room_type_is_not_draft(RoomTypeStatus status)
    {
        RoomType roomType = CreateRoomTypeWithStatus(status);

        Assert.Throws<DomainException>(roomType.Activate);

        Assert.Equal(status, roomType.Status);
    }

    [Fact]
    public void Suspend_changes_active_room_type_to_suspended()
    {
        RoomType roomType = CreateDraftRoomType();

        roomType.Activate();

        roomType.Suspend();

        Assert.Equal(RoomTypeStatus.Suspended, roomType.Status);
    }

    [Theory]
    [InlineData(RoomTypeStatus.Draft)]
    [InlineData(RoomTypeStatus.Suspended)]
    [InlineData(RoomTypeStatus.Closed)]
    public void Suspend_throws_exception_if_room_type_is_not_active(RoomTypeStatus status)
    {
        RoomType roomType = CreateRoomTypeWithStatus(status);

        Assert.Throws<DomainException>(roomType.Suspend);

        Assert.Equal(status, roomType.Status);
    }

    [Theory]
    [InlineData(RoomTypeStatus.Draft)]
    [InlineData(RoomTypeStatus.Active)]
    [InlineData(RoomTypeStatus.Suspended)]
    [InlineData(RoomTypeStatus.Closed)]
    public void Close_changes_any_room_type_to_closed(RoomTypeStatus status)
    {
        RoomType roomType = CreateRoomTypeWithStatus(status);

        roomType.Close();

        Assert.Equal(RoomTypeStatus.Closed, roomType.Status);
    }

    [Fact]
    public void Reactivate_changes_suspended_room_type_to_active()
    {
        RoomType roomType = CreateSuspendedRoomType();

        roomType.Reactivate();

        Assert.Equal(RoomTypeStatus.Active, roomType.Status);
    }

    [Theory]
    [InlineData(RoomTypeStatus.Draft)]
    [InlineData(RoomTypeStatus.Active)]
    [InlineData(RoomTypeStatus.Closed)]
    public void Reactivate_throws_exception_if_room_type_is_not_suspended(RoomTypeStatus status)
    {
        RoomType roomType = CreateRoomTypeWithStatus(status);

        Assert.Throws<DomainException>(roomType.Reactivate);

        Assert.Equal(status, roomType.Status);
    }

    [Theory]
    [InlineData(RoomTypeStatus.Draft, false)]
    [InlineData(RoomTypeStatus.Active, true)]
    [InlineData(RoomTypeStatus.Suspended, false)]
    [InlineData(RoomTypeStatus.Closed, false)]
    public void IsSellable_returns_true_only_when_room_type_is_active(RoomTypeStatus status, bool expected)
    {
        RoomType roomType = CreateRoomTypeWithStatus(status);

        Assert.Equal(expected, roomType.IsSellable);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(1, 0)]
    public void CanAccommodate_returns_true_when_guest_occupancy_fits_room_type(
        int adults,
        int children)
    {
        RoomType roomType = CreateRoomTypeWithStatus(RoomTypeStatus.Active);

        Assert.True(roomType.CanAccommodate(adults, children));
    }

    private static RoomType CreateRoomTypeWithStatus(RoomTypeStatus status)
    {
        return status switch {
            RoomTypeStatus.Draft => CreateDraftRoomType(),
            RoomTypeStatus.Active => CreateActiveRoomType(),
            RoomTypeStatus.Suspended => CreateSuspendedRoomType(),
            RoomTypeStatus.Closed => CreateClosedRoomType(),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private static RoomType CreateDraftRoomType()
    {
        return RoomType.CreateDraft(
            RoomTypeId.Create(),
            HotelId.Create(),
            "Standard Room",
            RoomTypeCode.Create("STD"),
            Occupancy.Create(2, 1, 2),
            [BedComposition.Create(BedType.Single, 1)],
            new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero));
    }

    private static RoomType CreateActiveRoomType()
    {
        RoomType roomType = CreateDraftRoomType();

        roomType.Activate();

        return roomType;
    }

    private static RoomType CreateSuspendedRoomType()
    {
        RoomType roomType = CreateActiveRoomType();

        roomType.Suspend();

        return roomType;
    }

    private static RoomType CreateClosedRoomType()
    {
        RoomType roomType = CreateActiveRoomType();

        roomType.Close();

        return roomType;
    }
}
