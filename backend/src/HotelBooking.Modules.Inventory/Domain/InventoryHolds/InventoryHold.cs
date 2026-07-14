using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

public sealed class InventoryHold : AggregateRoot<InventoryHoldId>
{
    public static readonly TimeSpan ExpirationWindow = TimeSpan.FromMinutes(10);

    private readonly List<InventoryHoldLine> _lines = [];

    public ReservationId ReservationId { get; private set; }
    public HotelId HotelId { get; private set; }
    public RoomTypeId RoomTypeId { get; private set; }
    public StayDateRange StayDateRange { get; private set; } = null!;
    public int Quantity { get; private set; }
    public InventoryHoldStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? ReleasedAt { get; private set; }
    public DateTimeOffset? ConfirmedAt { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }
    public IReadOnlyList<InventoryHoldLine> Lines => _lines.AsReadOnly();

    private InventoryHold()
    {
        // Required by EF Core
    }

    private InventoryHold(
        InventoryHoldId id,
        ReservationId reservationId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        StayDateRange stayDateRange,
        int quantity,
        DateTimeOffset createdAt,
        IReadOnlyCollection<InventoryHoldLine> lines)
    {
        Id = id;
        ReservationId = reservationId;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        StayDateRange = stayDateRange;
        Quantity = quantity;
        Status = InventoryHoldStatus.Held;
        CreatedAt = createdAt;
        ExpiresAt = createdAt.Add(ExpirationWindow);
        _lines = lines.ToList();
    }

    /// <summary>
    /// Creates a held inventory record after the application layer has atomically
    /// secured quantity for every occupied inventory date.
    /// </summary>
    /// <remarks>
    /// This method does not check availability or change inventory date quantities.
    /// </remarks>
    internal static InventoryHold CreateHeld(
        InventoryHoldId id,
        ReservationId reservationId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        StayDateRange stayDateRange,
        int quantity,
        DateTimeOffset createdAt)
    {
        ValidateCreateArguments(
            id,
            reservationId,
            hotelId,
            roomTypeId,
            stayDateRange,
            quantity,
            createdAt);

        IReadOnlyCollection<InventoryHoldLine> lines = stayDateRange
            .OccupiedDates
            .Select(occupiedDate => InventoryHoldLine.Create(
                InventoryHoldLineId.Create(),
                occupiedDate,
                quantity))
            .ToList();

        return new InventoryHold(
            id,
            reservationId,
            hotelId,
            roomTypeId,
            stayDateRange,
            quantity,
            createdAt,
            lines);
    }

    public bool IsExpiredAt(DateTimeOffset now)
    {
        return now >= ExpiresAt;
    }

    public void Release(DateTimeOffset releasedAt)
    {
        EnsureHeld("Only held inventory holds can be released.");
        ValidateTransitionTimestamp(releasedAt, nameof(releasedAt));

        if (releasedAt >= ExpiresAt)
        {
            throw new DomainException("Inventory hold cannot be released at or after its expiration time.");
        }

        ReleasedAt = releasedAt;
        Status = InventoryHoldStatus.Released;
    }

    public void Confirm(DateTimeOffset confirmedAt)
    {
        EnsureHeld("Only held inventory holds can be confirmed.");
        ValidateTransitionTimestamp(confirmedAt, nameof(confirmedAt));

        if (confirmedAt >= ExpiresAt)
        {
            throw new DomainException("Inventory hold cannot be confirmed at or after its expiration time.");
        }

        ConfirmedAt = confirmedAt;
        Status = InventoryHoldStatus.Confirmed;
    }

    public void Expire(DateTimeOffset expiredAt)
    {
        EnsureHeld("Only held inventory holds can be expired.");
        ValidateTransitionTimestamp(expiredAt, nameof(expiredAt));

        if (expiredAt < ExpiresAt)
        {
            throw new DomainException("Inventory hold cannot expire before its expiration time.");
        }

        ExpiredAt = expiredAt;
        Status = InventoryHoldStatus.Expired;
    }

    private void EnsureHeld(string message)
    {
        if (Status != InventoryHoldStatus.Held)
        {
            throw new DomainException(message);
        }
    }

    private void ValidateTransitionTimestamp(DateTimeOffset timestamp, string parameterName)
    {
        if (timestamp == default)
        {
            throw new DomainArgumentException("Transition timestamp is required.", parameterName);
        }

        if (timestamp < CreatedAt)
        {
            throw new DomainException("Inventory hold transition timestamp cannot be before creation time.");
        }
    }

    private static void ValidateCreateArguments(
        InventoryHoldId id,
        ReservationId reservationId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        StayDateRange stayDateRange,
        int quantity,
        DateTimeOffset createdAt)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Inventory hold ID is required.", nameof(id));
        }

        if (reservationId == default)
        {
            throw new DomainArgumentException("Reservation ID is required.", nameof(reservationId));
        }

        if (hotelId == default)
        {
            throw new DomainArgumentException("Hotel ID is required.", nameof(hotelId));
        }

        if (roomTypeId == default)
        {
            throw new DomainArgumentException("Room type ID is required.", nameof(roomTypeId));
        }

        if (stayDateRange is null)
        {
            throw new DomainArgumentException("Stay date range is required.", nameof(stayDateRange));
        }

        if (quantity <= 0)
        {
            throw new DomainArgumentException("Quantity must be greater than zero.", nameof(quantity));
        }

        if (createdAt == default)
        {
            throw new DomainArgumentException("Created at is required.", nameof(createdAt));
        }
    }
}
