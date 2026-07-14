using HotelBooking.Modules.Inventory.Domain.References;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

/// <summary>
/// Represents an atomic temporary claim on room type inventory for every occupied stay date.
/// </summary>
public sealed class InventoryHold : AggregateRoot<InventoryHoldId>
{
    /// <summary>Defines the first-version payment window for a held inventory claim.</summary>
    public static readonly TimeSpan ExpirationWindow = TimeSpan.FromMinutes(10);

    private readonly List<InventoryHoldLine> _lines = [];

    /// <summary>Gets the reservation that owns this hold.</summary>
    public ReservationId ReservationId { get; private set; }
    /// <summary>Gets the hotel whose inventory is held.</summary>
    public HotelId HotelId { get; private set; }
    /// <summary>Gets the held room type.</summary>
    public RoomTypeId RoomTypeId { get; private set; }
    /// <summary>Gets the stay date range covered by the hold.</summary>
    public StayDateRange StayDateRange { get; private set; } = null!;
    /// <summary>Gets the quantity held on every occupied date.</summary>
    public int Quantity { get; private set; }
    /// <summary>Gets the current hold lifecycle status.</summary>
    public InventoryHoldStatus Status { get; private set; }
    /// <summary>Gets the explicit time at which inventory was held.</summary>
    public DateTimeOffset CreatedAt { get; private set; }
    /// <summary>Gets the boundary at which the hold becomes expired.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }
    /// <summary>Gets when the hold was released, if released.</summary>
    public DateTimeOffset? ReleasedAt { get; private set; }
    /// <summary>Gets when the hold was confirmed, if confirmed.</summary>
    public DateTimeOffset? ConfirmedAt { get; private set; }
    /// <summary>Gets when the hold was marked expired, if expired.</summary>
    public DateTimeOffset? ExpiredAt { get; private set; }
    /// <summary>Gets the immutable occupied-date claims belonging to this hold.</summary>
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

    /// <summary>
    /// Determines whether the supplied explicit time has reached the expiration boundary.
    /// </summary>
    /// <param name="now">The time supplied by the application layer.</param>
    /// <returns><see langword="true"/> at or after expiration; otherwise <see langword="false"/>.</returns>
    public bool IsExpiredAt(DateTimeOffset now)
    {
        return now >= ExpiresAt;
    }

    /// <summary>
    /// Releases a held claim before it expires.
    /// </summary>
    /// <param name="releasedAt">The explicit release timestamp.</param>
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

    /// <summary>
    /// Confirms a held claim before it expires so inventory can become booked.
    /// </summary>
    /// <param name="confirmedAt">The explicit confirmation timestamp.</param>
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

    /// <summary>
    /// Marks a held claim as expired at or after its expiration boundary.
    /// </summary>
    /// <param name="expiredAt">The explicit expiration timestamp.</param>
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
