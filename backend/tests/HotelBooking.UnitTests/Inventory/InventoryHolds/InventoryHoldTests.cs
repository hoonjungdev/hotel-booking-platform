using System.Reflection;
using HotelBooking.Modules.Inventory.Domain.InventoryHolds;
using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Inventory.InventoryHolds;

public class InventoryHoldTests
{
    private static readonly DateTimeOffset CreatedAt = new(2026, 7, 1, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ExpiresAt = CreatedAt.AddMinutes(10);

    [Fact]
    public void CreateHeld_is_internal_to_inventory_module()
    {
        MethodInfo? createHeld = typeof(InventoryHold).GetMethod(
            nameof(InventoryHold.CreateHeld),
            BindingFlags.Static | BindingFlags.NonPublic);

        Assert.NotNull(createHeld);
        Assert.True(createHeld.IsAssembly);
    }

    [Fact]
    public void CreateHeld_records_successful_hold_with_one_line_per_occupied_date()
    {
        StayDateRange stayDateRange = CreateStayDateRange();
        InventoryHoldId holdId = InventoryHoldId.Create();
        ReservationId reservationId = ReservationId.Create();
        HotelId hotelId = HotelId.Create();
        RoomTypeId roomTypeId = RoomTypeId.Create();

        InventoryHold hold = InventoryHold.CreateHeld(
            id: holdId,
            reservationId: reservationId,
            hotelId: hotelId,
            roomTypeId: roomTypeId,
            stayDateRange: stayDateRange,
            quantity: 2,
            createdAt: CreatedAt);

        Assert.Equal(holdId, hold.Id);
        Assert.Equal(reservationId, hold.ReservationId);
        Assert.Equal(hotelId, hold.HotelId);
        Assert.Equal(roomTypeId, hold.RoomTypeId);
        Assert.Equal(stayDateRange, hold.StayDateRange);
        Assert.Equal(2, hold.Quantity);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
        Assert.Equal(CreatedAt, hold.CreatedAt);
        Assert.Equal(TimeSpan.FromMinutes(10), InventoryHold.ExpirationWindow);
        Assert.Equal(ExpiresAt, hold.ExpiresAt);
        Assert.Null(hold.ReleasedAt);
        Assert.Null(hold.ConfirmedAt);
        Assert.Null(hold.ExpiredAt);
        Assert.Equal(2, hold.Lines.Count);
        Assert.Equal([new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 2)], hold.Lines.Select(line => line.OccupiedDate));
        Assert.All(hold.Lines, line => Assert.Equal(2, line.Quantity));
    }

    [Fact]
    public void CreateHeld_throws_when_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: default,
                reservationId: ReservationId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: CreateStayDateRange(),
                quantity: 1,
                createdAt: CreatedAt));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void CreateHeld_throws_when_reservation_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: default,
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: CreateStayDateRange(),
                quantity: 1,
                createdAt: CreatedAt));

        Assert.Equal("reservationId", exception.ParamName);
    }

    [Fact]
    public void CreateHeld_throws_when_hotel_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: ReservationId.Create(),
                hotelId: default,
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: CreateStayDateRange(),
                quantity: 1,
                createdAt: CreatedAt));

        Assert.Equal("hotelId", exception.ParamName);
    }

    [Fact]
    public void CreateHeld_throws_when_room_type_id_is_empty()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: ReservationId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: default,
                stayDateRange: CreateStayDateRange(),
                quantity: 1,
                createdAt: CreatedAt));

        Assert.Equal("roomTypeId", exception.ParamName);
    }

    [Fact]
    public void CreateHeld_throws_when_stay_date_range_is_null()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: ReservationId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: null!,
                quantity: 1,
                createdAt: CreatedAt));

        Assert.Equal("stayDateRange", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateHeld_throws_when_quantity_is_not_positive(int quantity)
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: ReservationId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: CreateStayDateRange(),
                quantity: quantity,
                createdAt: CreatedAt));

        Assert.Equal("quantity", exception.ParamName);
    }

    [Fact]
    public void CreateHeld_throws_when_created_at_is_default()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            InventoryHold.CreateHeld(
                id: InventoryHoldId.Create(),
                reservationId: ReservationId.Create(),
                hotelId: HotelId.Create(),
                roomTypeId: RoomTypeId.Create(),
                stayDateRange: CreateStayDateRange(),
                quantity: 1,
                createdAt: default));

        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void IsExpiredAt_returns_false_before_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        Assert.False(hold.IsExpiredAt(ExpiresAt.AddTicks(-1)));
    }

    [Fact]
    public void IsExpiredAt_returns_true_at_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        Assert.True(hold.IsExpiredAt(ExpiresAt));
    }

    [Fact]
    public void Release_marks_hold_as_released()
    {
        InventoryHold hold = CreateInventoryHold();
        DateTimeOffset releasedAt = CreatedAt.AddMinutes(2);

        hold.Release(releasedAt);

        Assert.Equal(InventoryHoldStatus.Released, hold.Status);
        Assert.Equal(releasedAt, hold.ReleasedAt);
        Assert.Null(hold.ConfirmedAt);
        Assert.Null(hold.ExpiredAt);
    }

    [Fact]
    public void Release_throws_when_released_at_is_default()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() => hold.Release(default));

        Assert.Equal("releasedAt", exception.ParamName);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Release_throws_when_released_at_is_before_created_at()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainException exception = Assert.Throws<DomainException>(() => hold.Release(CreatedAt.AddTicks(-1)));

        Assert.Equal("Inventory hold transition timestamp cannot be before creation time.", exception.Message);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Release_throws_at_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainException exception = Assert.Throws<DomainException>(() => hold.Release(ExpiresAt));

        Assert.Equal("Inventory hold cannot be released at or after its expiration time.", exception.Message);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Confirm_marks_hold_as_confirmed_before_expiration()
    {
        InventoryHold hold = CreateInventoryHold();
        DateTimeOffset confirmedAt = ExpiresAt.AddTicks(-1);

        hold.Confirm(confirmedAt);

        Assert.Equal(InventoryHoldStatus.Confirmed, hold.Status);
        Assert.Equal(confirmedAt, hold.ConfirmedAt);
        Assert.Null(hold.ReleasedAt);
        Assert.Null(hold.ExpiredAt);
    }

    [Fact]
    public void Confirm_throws_when_confirmed_at_is_default()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() => hold.Confirm(default));

        Assert.Equal("confirmedAt", exception.ParamName);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Confirm_throws_when_confirmed_at_is_before_created_at()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainException exception = Assert.Throws<DomainException>(() => hold.Confirm(CreatedAt.AddTicks(-1)));

        Assert.Equal("Inventory hold transition timestamp cannot be before creation time.", exception.Message);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Confirm_throws_at_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainException exception = Assert.Throws<DomainException>(() => hold.Confirm(ExpiresAt));

        Assert.Equal("Inventory hold cannot be confirmed at or after its expiration time.", exception.Message);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Expire_marks_hold_as_expired_at_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        hold.Expire(ExpiresAt);

        Assert.Equal(InventoryHoldStatus.Expired, hold.Status);
        Assert.Equal(ExpiresAt, hold.ExpiredAt);
        Assert.Null(hold.ReleasedAt);
        Assert.Null(hold.ConfirmedAt);
    }

    [Fact]
    public void Expire_throws_when_expired_at_is_default()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() => hold.Expire(default));

        Assert.Equal("expiredAt", exception.ParamName);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Expire_throws_before_expiration()
    {
        InventoryHold hold = CreateInventoryHold();

        DomainException exception = Assert.Throws<DomainException>(() => hold.Expire(ExpiresAt.AddTicks(-1)));

        Assert.Equal("Inventory hold cannot expire before its expiration time.", exception.Message);
        Assert.Equal(InventoryHoldStatus.Held, hold.Status);
    }

    [Fact]
    public void Confirm_throws_when_hold_is_released()
    {
        InventoryHold hold = CreateInventoryHold();
        hold.Release(CreatedAt.AddMinutes(1));

        DomainException exception = Assert.Throws<DomainException>(() => hold.Confirm(CreatedAt.AddMinutes(2)));

        Assert.Equal("Only held inventory holds can be confirmed.", exception.Message);
    }

    [Fact]
    public void Release_throws_when_hold_is_confirmed()
    {
        InventoryHold hold = CreateInventoryHold();
        hold.Confirm(CreatedAt.AddMinutes(1));

        DomainException exception = Assert.Throws<DomainException>(() => hold.Release(CreatedAt.AddMinutes(2)));

        Assert.Equal("Only held inventory holds can be released.", exception.Message);
    }

    [Fact]
    public void Release_throws_when_hold_is_expired()
    {
        InventoryHold hold = CreateInventoryHold();
        hold.Expire(ExpiresAt);

        DomainException exception = Assert.Throws<DomainException>(() => hold.Release(ExpiresAt.AddMinutes(1)));

        Assert.Equal("Only held inventory holds can be released.", exception.Message);
    }

    [Fact]
    public void Expire_throws_when_hold_is_confirmed()
    {
        InventoryHold hold = CreateInventoryHold();
        hold.Confirm(CreatedAt.AddMinutes(1));

        DomainException exception = Assert.Throws<DomainException>(() => hold.Expire(ExpiresAt));

        Assert.Equal("Only held inventory holds can be expired.", exception.Message);
    }

    private static InventoryHold CreateInventoryHold()
    {
        return InventoryHold.CreateHeld(
            id: InventoryHoldId.Create(),
            reservationId: ReservationId.Create(),
            hotelId: HotelId.Create(),
            roomTypeId: RoomTypeId.Create(),
            stayDateRange: CreateStayDateRange(),
            quantity: 1,
            createdAt: CreatedAt);
    }

    private static StayDateRange CreateStayDateRange()
    {
        return new StayDateRange(
            checkInDate: new DateOnly(2026, 7, 1),
            checkOutDate: new DateOnly(2026, 7, 3));
    }
}
