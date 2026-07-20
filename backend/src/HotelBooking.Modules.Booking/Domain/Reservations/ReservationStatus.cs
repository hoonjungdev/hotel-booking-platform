namespace HotelBooking.Modules.Booking.Domain.Reservations;

/// <summary>Identifies the current stage of a Reservation lifecycle.</summary>
public enum ReservationStatus
{
    /// <summary>The Reservation exists but its Inventory Hold outcome is unresolved.</summary>
    Pending = 1,

    /// <summary>Inventory is held and the Reservation is waiting for Payment.</summary>
    AwaitingPayment = 2,

    /// <summary>The Inventory Hold failed, so the Reservation cannot continue.</summary>
    Failed = 3
}
