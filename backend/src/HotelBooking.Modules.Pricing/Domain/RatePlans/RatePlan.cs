using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Pricing.Domain.RatePlans;

/// <summary>
/// Represents a lifecycle-managed pricing option for one room type under a cancellation policy.
/// </summary>
public sealed class RatePlan : AggregateRoot<RatePlanId>
{
    /// <summary>Gets the immutable hotel facts under which this rate plan was defined.</summary>
    public HotelRateSettings HotelRateSettings { get; private set; } = null!;

    /// <summary>Gets the hotel that offers this rate plan.</summary>
    public HotelId HotelId => HotelRateSettings.HotelId;

    /// <summary>Gets the room type sold through this rate plan.</summary>
    public RoomTypeId RoomTypeId { get; private set; }

    /// <summary>Gets the immutable currency in which this rate plan is sold.</summary>
    public Currency SellingCurrency => HotelRateSettings.SellingCurrency;

    /// <summary>Gets the guest-facing rate plan name.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Gets the hotel-specific rate plan code.</summary>
    public RatePlanCode Code { get; private set; } = null!;

    /// <summary>Gets the agreed cancellation penalty schedule for this pricing option.</summary>
    public CancellationPolicy CancellationPolicy { get; private set; } = null!;

    /// <summary>Gets the current rate plan lifecycle status.</summary>
    public RatePlanStatus Status { get; private set; }

    private RatePlan()
    {
        // Required by EF Core
    }

    private RatePlan(
        RatePlanId id,
        HotelRateSettings hotelRateSettings,
        RoomTypeId roomTypeId,
        string name,
        RatePlanCode code,
        CancellationPolicy cancellationPolicy)
    {
        Id = id;
        HotelRateSettings = hotelRateSettings;
        RoomTypeId = roomTypeId;
        Name = name;
        Code = code;
        CancellationPolicy = cancellationPolicy;
        Status = RatePlanStatus.Draft;
    }

    /// <summary>Creates a draft rate plan for an existing hotel and room type.</summary>
    /// <param name="id">The Pricing-owned RatePlan identifier.</param>
    /// <param name="hotelRateSettings">
    /// The hotel identifier and selling currency obtained together at the Pricing boundary.
    /// </param>
    /// <param name="roomTypeId">The existing room type identifier owned by the Property module.</param>
    /// <param name="name">The required guest-facing name.</param>
    /// <param name="code">The required hotel-defined operational code.</param>
    /// <param name="cancellationPolicy">The required cancellation penalty schedule.</param>
    /// <returns>A valid RatePlan in <see cref="RatePlanStatus.Draft"/> status.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when an identifier, hotel rate settings, name, code, or cancellation policy is missing.
    /// </exception>
    public static RatePlan CreateDraft(
        RatePlanId id,
        HotelRateSettings hotelRateSettings,
        RoomTypeId roomTypeId,
        string name,
        RatePlanCode code,
        CancellationPolicy cancellationPolicy)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Rate plan ID is required.", nameof(id));
        }

        if (hotelRateSettings is null)
        {
            throw new DomainArgumentException(
                "Hotel rate settings are required.",
                nameof(hotelRateSettings));
        }

        if (roomTypeId == default)
        {
            throw new DomainArgumentException("Room type ID is required.", nameof(roomTypeId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainArgumentException("Name is required.", nameof(name));
        }

        string normalizedName = name.Trim();

        if (code is null)
        {
            throw new DomainArgumentException("Code is required.", nameof(code));
        }

        if (cancellationPolicy is null)
        {
            throw new DomainArgumentException(
                "Cancellation policy is required.",
                nameof(cancellationPolicy));
        }

        return new RatePlan(
            id,
            hotelRateSettings,
            roomTypeId,
            normalizedName,
            code,
            cancellationPolicy);
    }

    /// <summary>Marks a fully configured draft RatePlan as active at the RatePlan level.</summary>
    /// <exception cref="DomainException">Thrown when the RatePlan is not in Draft status.</exception>
    public void Activate()
    {
        if (Status != RatePlanStatus.Draft)
        {
            throw new DomainException(
                "Rate plan cannot be activated from the current status. Current status: " + Status);
        }

        Status = RatePlanStatus.Active;
    }

    /// <summary>Temporarily disables an active RatePlan at the RatePlan level.</summary>
    /// <exception cref="DomainException">Thrown when the RatePlan is not Active.</exception>
    public void Suspend()
    {
        if (Status != RatePlanStatus.Active)
        {
            throw new DomainException(
                "Rate plan cannot be suspended from the current status. Current status: " + Status);
        }

        Status = RatePlanStatus.Suspended;
    }

    /// <summary>Returns a suspended RatePlan to Active status.</summary>
    /// <exception cref="DomainException">Thrown when the RatePlan is not Suspended.</exception>
    public void Reactivate()
    {
        if (Status != RatePlanStatus.Suspended)
        {
            throw new DomainException(
                "Rate plan cannot be reactivated from the current status. Current status: " + Status);
        }

        Status = RatePlanStatus.Active;
    }

    /// <summary>Permanently closes a suspended RatePlan; repeated closure is idempotent.</summary>
    /// <exception cref="DomainException">
    /// Thrown when an open RatePlan has not first been suspended.
    /// </exception>
    public void Close()
    {
        if (Status == RatePlanStatus.Closed)
        {
            return;
        }

        if (Status != RatePlanStatus.Suspended)
        {
            throw new DomainException(
                "Rate plan must be suspended before it can be closed. Current status: " + Status);
        }

        Status = RatePlanStatus.Closed;
    }
}
