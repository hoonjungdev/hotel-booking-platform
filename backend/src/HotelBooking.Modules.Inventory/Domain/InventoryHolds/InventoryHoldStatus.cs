namespace HotelBooking.Modules.Inventory.Domain.InventoryHolds;

/// <summary>Describes the lifecycle of a temporary inventory claim.</summary>
public enum InventoryHoldStatus
{
    /// <summary>The quantity is claimed and awaiting a terminal outcome.</summary>
    Held = 1,
    /// <summary>The claim was released before expiration.</summary>
    Released = 2,
    /// <summary>The claim was converted toward booked inventory.</summary>
    Confirmed = 3,
    /// <summary>The payment window elapsed and the claim expired.</summary>
    Expired = 4
}
