using HotelBooking.Modules.Booking.Domain.References;
using HotelBooking.Modules.Booking.Domain.Reservations;
using HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.UnitTests.Booking.Reservations;

/// <summary>Proves Reservation creation and Inventory Hold outcome state transitions.</summary>
public class ReservationTests
{
    private static readonly ReservationId ExistingReservationId =
        ReservationId.From(Guid.Parse("70000000-0000-0000-0000-000000000001"));
    private static readonly GuestId ExistingGuestId =
        GuestId.From(Guid.Parse("80000000-0000-0000-0000-000000000001"));
    private static readonly DateTimeOffset CreatedAt =
        new(2026, 8, 1, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_starts_a_complete_reservation_as_pending()
    {
        ReservationPriceSnapshot priceSnapshot = CreatePriceSnapshot();

        Reservation reservation = Reservation.Create(
            ExistingReservationId,
            ExistingGuestId,
            priceSnapshot,
            CreatedAt);

        Assert.Equal(ExistingReservationId, reservation.Id);
        Assert.Equal(ExistingGuestId, reservation.GuestId);
        Assert.Same(priceSnapshot, reservation.PriceSnapshot);
        Assert.Equal(priceSnapshot.PriceQuoteId, reservation.PriceQuoteId);
        Assert.Equal(priceSnapshot.HotelId, reservation.HotelId);
        Assert.Equal(priceSnapshot.RoomTypeId, reservation.RoomTypeId);
        Assert.Equal(priceSnapshot.RatePlanId, reservation.RatePlanId);
        Assert.Equal(priceSnapshot.StayDateRange, reservation.StayDateRange);
        Assert.Equal(priceSnapshot.RequestedOccupancy, reservation.RequestedOccupancy);
        Assert.Equal(priceSnapshot.TotalPrice, reservation.TotalPrice);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(CreatedAt, reservation.CreatedAt);
        Assert.Null(reservation.InventoryHeldAt);
        Assert.Null(reservation.FailedAt);
    }

    [Fact]
    public void Create_rejects_a_default_identifier()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            Reservation.Create(
                default,
                ExistingGuestId,
                CreatePriceSnapshot(),
                CreatedAt));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_default_guest_identifier()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            Reservation.Create(
                ExistingReservationId,
                default,
                CreatePriceSnapshot(),
                CreatedAt));

        Assert.Equal("guestId", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_price_snapshot()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            Reservation.Create(
                ExistingReservationId,
                ExistingGuestId,
                null!,
                CreatedAt));

        Assert.Equal("priceSnapshot", exception.ParamName);
    }

    [Fact]
    public void Create_rejects_a_missing_creation_time()
    {
        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            Reservation.Create(
                ExistingReservationId,
                ExistingGuestId,
                CreatePriceSnapshot(),
                default));

        Assert.Equal("createdAt", exception.ParamName);
    }

    [Fact]
    public void MarkInventoryHeld_moves_a_pending_reservation_to_awaiting_payment()
    {
        Reservation reservation = CreateReservation();
        DateTimeOffset heldAt = CreatedAt.AddMinutes(1);

        reservation.MarkInventoryHeld(heldAt);

        Assert.Equal(ReservationStatus.AwaitingPayment, reservation.Status);
        Assert.Equal(heldAt, reservation.InventoryHeldAt);
        Assert.Null(reservation.FailedAt);
    }

    [Fact]
    public void MarkInventoryHeld_allows_the_creation_time_boundary()
    {
        Reservation reservation = CreateReservation();

        reservation.MarkInventoryHeld(CreatedAt);

        Assert.Equal(ReservationStatus.AwaitingPayment, reservation.Status);
        Assert.Equal(CreatedAt, reservation.InventoryHeldAt);
    }

    [Fact]
    public void MarkInventoryHeld_rejects_a_missing_result_time_without_changing_state()
    {
        Reservation reservation = CreateReservation();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            reservation.MarkInventoryHeld(default));

        Assert.Equal("heldAt", exception.ParamName);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Null(reservation.InventoryHeldAt);
    }

    [Fact]
    public void MarkInventoryHeld_rejects_a_result_before_creation_without_changing_state()
    {
        Reservation reservation = CreateReservation();

        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHeld(CreatedAt.AddTicks(-1)));

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Null(reservation.InventoryHeldAt);
    }

    [Fact]
    public void MarkInventoryHoldFailed_moves_a_pending_reservation_to_failed()
    {
        Reservation reservation = CreateReservation();
        DateTimeOffset failedAt = CreatedAt.AddMinutes(1);

        reservation.MarkInventoryHoldFailed(failedAt);

        Assert.Equal(ReservationStatus.Failed, reservation.Status);
        Assert.Equal(failedAt, reservation.FailedAt);
        Assert.Null(reservation.InventoryHeldAt);
    }

    [Fact]
    public void MarkInventoryHoldFailed_allows_the_creation_time_boundary()
    {
        Reservation reservation = CreateReservation();

        reservation.MarkInventoryHoldFailed(CreatedAt);

        Assert.Equal(ReservationStatus.Failed, reservation.Status);
        Assert.Equal(CreatedAt, reservation.FailedAt);
    }

    [Fact]
    public void MarkInventoryHoldFailed_rejects_a_missing_result_time_without_changing_state()
    {
        Reservation reservation = CreateReservation();

        DomainArgumentException exception = Assert.Throws<DomainArgumentException>(() =>
            reservation.MarkInventoryHoldFailed(default));

        Assert.Equal("failedAt", exception.ParamName);
        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Null(reservation.FailedAt);
    }

    [Fact]
    public void MarkInventoryHoldFailed_rejects_a_result_before_creation_without_changing_state()
    {
        Reservation reservation = CreateReservation();

        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHoldFailed(CreatedAt.AddTicks(-1)));

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Null(reservation.FailedAt);
    }

    [Fact]
    public void Awaiting_payment_reservation_rejects_another_inventory_result()
    {
        Reservation reservation = CreateReservation();
        reservation.MarkInventoryHeld(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHeld(CreatedAt.AddMinutes(2)));
        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHoldFailed(CreatedAt.AddMinutes(2)));

        Assert.Equal(ReservationStatus.AwaitingPayment, reservation.Status);
        Assert.Equal(CreatedAt.AddMinutes(1), reservation.InventoryHeldAt);
        Assert.Null(reservation.FailedAt);
    }

    [Fact]
    public void Failed_reservation_rejects_another_inventory_result()
    {
        Reservation reservation = CreateReservation();
        reservation.MarkInventoryHoldFailed(CreatedAt.AddMinutes(1));

        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHeld(CreatedAt.AddMinutes(2)));
        Assert.Throws<DomainException>(() =>
            reservation.MarkInventoryHoldFailed(CreatedAt.AddMinutes(2)));

        Assert.Equal(ReservationStatus.Failed, reservation.Status);
        Assert.Equal(CreatedAt.AddMinutes(1), reservation.FailedAt);
        Assert.Null(reservation.InventoryHeldAt);
    }

    private static Reservation CreateReservation()
    {
        return Reservation.Create(
            ExistingReservationId,
            ExistingGuestId,
            CreatePriceSnapshot(),
            CreatedAt);
    }

    private static ReservationPriceSnapshot CreatePriceSnapshot()
    {
        StayDateRange stay = new(
            new DateOnly(2026, 8, 15),
            new DateOnly(2026, 8, 17));
        Currency currency = Currency.FromCode("KRW");

        return ReservationPriceSnapshot.Create(
            PriceQuoteId.From(Guid.Parse("90000000-0000-0000-0000-000000000001")),
            HotelId.From(Guid.Parse("90000000-0000-0000-0000-000000000002")),
            RoomTypeId.From(Guid.Parse("90000000-0000-0000-0000-000000000003")),
            RatePlanId.From(Guid.Parse("90000000-0000-0000-0000-000000000004")),
            stay,
            RequestedOccupancy.Create(2, 1),
            [
                ReservationNightlyPrice.Create(
                    stay.CheckInDate,
                    Money.Create(150_000m, currency)),
                ReservationNightlyPrice.Create(
                    stay.CheckInDate.AddDays(1),
                    Money.Create(180_000m, currency))
            ],
            CancellationPolicySnapshot.Create(
                new CancellationRuleSnapshot(
                    TimeSpan.FromDays(3),
                    CancellationPenaltySnapshot.NoPenalty()),
                new CancellationRuleSnapshot(
                    TimeSpan.Zero,
                    CancellationPenaltySnapshot.FullStay())));
    }
}
