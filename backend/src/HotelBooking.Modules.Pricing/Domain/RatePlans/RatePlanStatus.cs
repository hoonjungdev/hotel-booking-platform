namespace HotelBooking.Modules.Pricing.Domain.RatePlans;

/// <summary>
/// Describes the RatePlan's own lifecycle state without asserting complete guest availability.
/// </summary>
public enum RatePlanStatus
{
    /// <summary>The RatePlan is being configured and has not been enabled.</summary>
    Draft = 1,
    /// <summary>The RatePlan is enabled for use in broader sale-eligibility checks.</summary>
    Active = 2,
    /// <summary>The RatePlan is temporarily disabled.</summary>
    Suspended = 3,
    /// <summary>The RatePlan is permanently closed.</summary>
    Closed = 4
}
