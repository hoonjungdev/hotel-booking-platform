using HotelBooking.Modules.Booking.Domain.References;
using HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Booking.Domain.Reservations;

/// <summary>Governs a Guest's request to reserve one Room Type under agreed price terms.</summary>
public sealed class Reservation : AggregateRoot<ReservationId>
{
    /// <summary>Gets the authenticated Guest that created the Reservation.</summary>
    public GuestId GuestId { get; private set; }

    /// <summary>Gets the immutable price and terms agreed at Reservation creation.</summary>
    public ReservationPriceSnapshot PriceSnapshot { get; private set; } = null!;

    /// <summary>Gets the current Reservation lifecycle status.</summary>
    public ReservationStatus Status { get; private set; }

    /// <summary>Gets the explicit time at which the Reservation was created.</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>Gets when Inventory Hold success moved the Reservation to Awaiting Payment.</summary>
    public DateTimeOffset? InventoryHeldAt { get; private set; }

    /// <summary>Gets when an Inventory Hold failure made the Reservation unable to continue.</summary>
    public DateTimeOffset? FailedAt { get; private set; }

    /// <summary>Gets the accepted Price Quote identifier.</summary>
    public PriceQuoteId PriceQuoteId => PriceSnapshot.PriceQuoteId;

    /// <summary>Gets the Hotel selected by the Guest.</summary>
    public HotelId HotelId => PriceSnapshot.HotelId;

    /// <summary>Gets the Room Type requested by the Guest.</summary>
    public RoomTypeId RoomTypeId => PriceSnapshot.RoomTypeId;

    /// <summary>Gets the Rate Plan accepted by the Guest.</summary>
    public RatePlanId RatePlanId => PriceSnapshot.RatePlanId;

    /// <summary>Gets the agreed Stay Date Range.</summary>
    public StayDateRange StayDateRange => PriceSnapshot.StayDateRange;

    /// <summary>Gets the agreed Requested Occupancy.</summary>
    public RequestedOccupancy RequestedOccupancy => PriceSnapshot.RequestedOccupancy;

    /// <summary>Gets the final agreed total price.</summary>
    public Money TotalPrice => PriceSnapshot.TotalPrice;

    private Reservation()
    {
        // Required by EF Core
    }

    private Reservation(
        ReservationId id,
        GuestId guestId,
        ReservationPriceSnapshot priceSnapshot,
        DateTimeOffset createdAt)
    {
        Id = id;
        GuestId = guestId;
        PriceSnapshot = priceSnapshot;
        Status = ReservationStatus.Pending;
        CreatedAt = createdAt;
    }

    /// <summary>Creates a Pending Reservation from complete, already accepted price terms.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when an identifier, price snapshot, or creation time is missing.
    /// </exception>
    public static Reservation Create(
        ReservationId id,
        GuestId guestId,
        ReservationPriceSnapshot priceSnapshot,
        DateTimeOffset createdAt)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Reservation ID is required.", nameof(id));
        }

        if (guestId == default)
        {
            throw new DomainArgumentException("Guest ID is required.", nameof(guestId));
        }

        if (priceSnapshot is null)
        {
            throw new DomainArgumentException(
                "Reservation Price Snapshot is required.",
                nameof(priceSnapshot));
        }

        if (createdAt == default)
        {
            throw new DomainArgumentException(
                "Reservation creation time is required.",
                nameof(createdAt));
        }

        return new Reservation(id, guestId, priceSnapshot, createdAt);
    }

    /// <summary>Records successful Inventory Hold and moves a Pending Reservation to Awaiting Payment.</summary>
    /// <exception cref="DomainArgumentException">Thrown when the result time is missing.</exception>
    /// <exception cref="DomainException">
    /// Thrown when the result predates creation or the Reservation is not Pending.
    /// </exception>
    public void MarkInventoryHeld(DateTimeOffset heldAt)
    {
        EnsurePending("Only a Pending Reservation can record a successful Inventory Hold.");
        ValidateInventoryResultTime(heldAt, nameof(heldAt));

        InventoryHeldAt = heldAt;
        Status = ReservationStatus.AwaitingPayment;
    }

    /// <summary>Records failed Inventory Hold and moves a Pending Reservation to Failed.</summary>
    /// <exception cref="DomainArgumentException">Thrown when the result time is missing.</exception>
    /// <exception cref="DomainException">
    /// Thrown when the result predates creation or the Reservation is not Pending.
    /// </exception>
    public void MarkInventoryHoldFailed(DateTimeOffset failedAt)
    {
        EnsurePending("Only a Pending Reservation can record a failed Inventory Hold.");
        ValidateInventoryResultTime(failedAt, nameof(failedAt));

        FailedAt = failedAt;
        Status = ReservationStatus.Failed;
    }

    private void EnsurePending(string message)
    {
        if (Status != ReservationStatus.Pending)
        {
            throw new DomainException(message);
        }
    }

    private void ValidateInventoryResultTime(DateTimeOffset resultAt, string parameterName)
    {
        if (resultAt == default)
        {
            throw new DomainArgumentException(
                "Inventory Hold result time is required.",
                parameterName);
        }

        if (resultAt < CreatedAt)
        {
            throw new DomainException(
                "Inventory Hold result time cannot be before Reservation creation time.");
        }
    }
}
