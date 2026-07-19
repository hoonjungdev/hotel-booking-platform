using HotelBooking.Modules.Pricing.Domain.DailyRates;
using HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.Modules.Pricing.Domain.RatePlans.ValueObjects;
using HotelBooking.Modules.Pricing.Domain.References;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Pricing.Domain.PriceQuotes;

/// <summary>
/// Preserves a time-limited nightly price and cancellation-term offer for one requested room.
/// </summary>
public sealed class PriceQuote : AggregateRoot<PriceQuoteId>
{
    private readonly List<NightlyPrice> _nightlyPrices = [];

    /// <summary>Gets the hotel that issued this price offer.</summary>
    public HotelId HotelId { get; private set; }

    /// <summary>Gets the room type priced by this offer.</summary>
    public RoomTypeId RoomTypeId { get; private set; }

    /// <summary>Gets the active RatePlan whose terms were used for this offer.</summary>
    public RatePlanId RatePlanId { get; private set; }

    /// <summary>Gets the stay whose occupied dates are priced by this offer.</summary>
    public StayDateRange StayDateRange { get; private set; } = null!;

    /// <summary>Gets the guest composition requested for the single quoted room.</summary>
    public RequestedOccupancy RequestedOccupancy { get; private set; } = null!;

    /// <summary>Gets the immutable nightly price snapshots in occupied-date order.</summary>
    public IReadOnlyList<NightlyPrice> NightlyPrices => _nightlyPrices.AsReadOnly();

    /// <summary>Gets the sum of every nightly price in the RatePlan selling currency.</summary>
    public Money TotalPrice { get; private set; } = null!;

    /// <summary>Gets the cancellation terms offered with this price.</summary>
    public CancellationPolicy CancellationPolicy { get; private set; } = null!;

    /// <summary>Gets the explicit instant at which this offer was issued.</summary>
    public DateTimeOffset QuotedAt { get; private set; }

    /// <summary>Gets the boundary instant at which this price offer stops being valid.</summary>
    public DateTimeOffset ExpiresAt { get; private set; }

    private PriceQuote()
    {
        // Required by EF Core
    }

    private PriceQuote(
        PriceQuoteId id,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        RatePlanId ratePlanId,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        IReadOnlyCollection<NightlyPrice> nightlyPrices,
        Money totalPrice,
        CancellationPolicy cancellationPolicy,
        DateTimeOffset quotedAt,
        DateTimeOffset expiresAt)
    {
        Id = id;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        RatePlanId = ratePlanId;
        StayDateRange = stayDateRange;
        RequestedOccupancy = requestedOccupancy;
        _nightlyPrices = nightlyPrices.ToList();
        TotalPrice = totalPrice;
        CancellationPolicy = cancellationPolicy;
        QuotedAt = quotedAt;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// Creates a time-limited price offer from one active RatePlan and the exact DailyRates
    /// covering a stay.
    /// </summary>
    /// <param name="id">The Pricing-owned quote identifier.</param>
    /// <param name="ratePlan">The active RatePlan whose identity, currency, and terms apply.</param>
    /// <param name="stayDateRange">The included check-in and excluded check-out dates.</param>
    /// <param name="requestedOccupancy">The guest composition for the single quoted room.</param>
    /// <param name="dailyRates">Exactly one matching DailyRate for every occupied date.</param>
    /// <param name="quotedAt">The explicit instant at which the offer was issued.</param>
    /// <param name="expiresAt">The later instant at which the offer becomes expired.</param>
    /// <returns>An immutable PriceQuote with ordered nightly prices and a calculated total.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when a required argument is missing.
    /// </exception>
    /// <exception cref="DomainException">
    /// Thrown when the RatePlan is inactive, the validity interval is invalid, or the supplied
    /// DailyRates do not exactly and consistently price the stay.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the nightly price sum exceeds the range supported by <see cref="decimal"/>.
    /// </exception>
    public static PriceQuote Create(
        PriceQuoteId id,
        RatePlan ratePlan,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        IReadOnlyCollection<DailyRate> dailyRates,
        DateTimeOffset quotedAt,
        DateTimeOffset expiresAt)
    {
        ValidateRequiredArguments(
            id,
            ratePlan,
            stayDateRange,
            requestedOccupancy,
            dailyRates,
            quotedAt,
            expiresAt);

        if (ratePlan.Status != RatePlanStatus.Active)
        {
            throw new DomainException(
                "A price quote requires an active RatePlan. Current status: " + ratePlan.Status);
        }

        if (expiresAt <= quotedAt)
        {
            throw new DomainException("Price quote expiration must be after its issue time.");
        }

        ValidateDailyRates(ratePlan, stayDateRange, dailyRates);

        List<NightlyPrice> nightlyPrices = dailyRates
            .OrderBy(dailyRate => dailyRate.OccupiedDate)
            .Select(dailyRate => NightlyPrice.Create(
                dailyRate.OccupiedDate,
                dailyRate.Price))
            .ToList();

        Money totalPrice = nightlyPrices.Aggregate(
            Money.Zero(ratePlan.SellingCurrency),
            (total, nightlyPrice) => total.Add(nightlyPrice.Price));

        CancellationPolicy cancellationPolicySnapshot = CancellationPolicy.Create(
            ratePlan.CancellationPolicy.Rules.ToArray());

        return new PriceQuote(
            id,
            ratePlan.HotelId,
            ratePlan.RoomTypeId,
            ratePlan.Id,
            stayDateRange,
            requestedOccupancy,
            nightlyPrices,
            totalPrice,
            cancellationPolicySnapshot,
            quotedAt,
            expiresAt);
    }

    /// <summary>Determines whether an explicit instant has reached this quote's expiration boundary.</summary>
    /// <param name="now">The explicit instant supplied by the application layer.</param>
    /// <returns><see langword="true"/> at or after expiration; otherwise <see langword="false"/>.</returns>
    /// <exception cref="DomainArgumentException">Thrown when the instant is missing.</exception>
    public bool IsExpiredAt(DateTimeOffset now)
    {
        if (now == default)
        {
            throw new DomainArgumentException(
                "Price quote evaluation time is required.",
                nameof(now));
        }

        return now >= ExpiresAt;
    }

    /// <summary>Ensures every value required to issue a complete PriceQuote is present.</summary>
    /// <param name="id">The quote identifier being validated.</param>
    /// <param name="ratePlan">The RatePlan being quoted.</param>
    /// <param name="stayDateRange">The requested stay.</param>
    /// <param name="requestedOccupancy">The requested guest composition.</param>
    /// <param name="dailyRates">The DailyRates supplied for the stay.</param>
    /// <param name="quotedAt">The explicit quote issue time.</param>
    /// <param name="expiresAt">The explicit quote expiration time.</param>
    /// <exception cref="DomainArgumentException">
    /// Thrown when a required value is missing or the DailyRate collection contains a missing item.
    /// </exception>
    private static void ValidateRequiredArguments(
        PriceQuoteId id,
        RatePlan ratePlan,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        IReadOnlyCollection<DailyRate> dailyRates,
        DateTimeOffset quotedAt,
        DateTimeOffset expiresAt)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Price quote ID is required.", nameof(id));
        }

        if (ratePlan is null)
        {
            throw new DomainArgumentException("RatePlan is required.", nameof(ratePlan));
        }

        if (stayDateRange is null)
        {
            throw new DomainArgumentException(
                "Stay date range is required.",
                nameof(stayDateRange));
        }

        if (requestedOccupancy is null)
        {
            throw new DomainArgumentException(
                "Requested occupancy is required.",
                nameof(requestedOccupancy));
        }

        if (dailyRates is null)
        {
            throw new DomainArgumentException("DailyRates are required.", nameof(dailyRates));
        }

        if (dailyRates.Any(dailyRate => dailyRate is null))
        {
            throw new DomainArgumentException(
                "DailyRates cannot contain a missing DailyRate.",
                nameof(dailyRates));
        }

        if (quotedAt == default)
        {
            throw new DomainArgumentException("Quoted at is required.", nameof(quotedAt));
        }

        if (expiresAt == default)
        {
            throw new DomainArgumentException("Expires at is required.", nameof(expiresAt));
        }
    }

    /// <summary>
    /// Ensures the supplied DailyRates consistently and completely price every occupied date.
    /// </summary>
    /// <param name="ratePlan">The RatePlan whose identity and selling currency must match.</param>
    /// <param name="stayDateRange">The stay whose occupied dates must be priced exactly once.</param>
    /// <param name="dailyRates">The DailyRates being included in the quote.</param>
    /// <exception cref="DomainException">
    /// Thrown when a DailyRate has another RatePlan or currency, is duplicated, falls outside
    /// the stay, or leaves an occupied date unpriced.
    /// </exception>
    private static void ValidateDailyRates(
        RatePlan ratePlan,
        StayDateRange stayDateRange,
        IReadOnlyCollection<DailyRate> dailyRates)
    {
        if (dailyRates.Any(dailyRate => dailyRate.RatePlanId != ratePlan.Id))
        {
            throw new DomainException(
                "Every DailyRate in a price quote must belong to its RatePlan.");
        }

        if (dailyRates.Any(dailyRate => dailyRate.Price.Currency != ratePlan.SellingCurrency))
        {
            throw new DomainException(
                "Every DailyRate in a price quote must use the RatePlan selling currency.");
        }

        if (dailyRates
            .GroupBy(dailyRate => dailyRate.OccupiedDate)
            .Any(group => group.Count() > 1))
        {
            throw new DomainException(
                "A price quote cannot contain duplicate DailyRates for an occupied date.");
        }

        HashSet<DateOnly> expectedOccupiedDates = stayDateRange.OccupiedDates.ToHashSet();

        if (dailyRates.Any(dailyRate => !expectedOccupiedDates.Contains(dailyRate.OccupiedDate)))
        {
            throw new DomainException(
                "A price quote cannot contain a DailyRate outside its stay date range.");
        }

        HashSet<DateOnly> suppliedOccupiedDates = dailyRates
            .Select(dailyRate => dailyRate.OccupiedDate)
            .ToHashSet();

        if (!expectedOccupiedDates.SetEquals(suppliedOccupiedDates))
        {
            throw new DomainException(
                "A price quote requires one DailyRate for every occupied date.");
        }
    }
}
