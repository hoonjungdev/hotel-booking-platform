using HotelBooking.Modules.Pricing.Domain.RatePlans;
using HotelBooking.SharedKernel.Domain;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Pricing.Domain.DailyRates;

/// <summary>
/// Governs the price of one RatePlan on one occupied date.
/// </summary>
public sealed class DailyRate : AggregateRoot<DailyRateId>
{
    /// <summary>Gets the RatePlan priced by this DailyRate.</summary>
    public RatePlanId RatePlanId { get; private set; }

    /// <summary>Gets the specific date on which the priced room type is occupied.</summary>
    public DateOnly OccupiedDate { get; private set; }

    /// <summary>Gets the price in the RatePlan's immutable selling currency.</summary>
    public Money Price { get; private set; } = null!;

    private DailyRate()
    {
        // Required by EF Core
    }

    private DailyRate(
        DailyRateId id,
        RatePlanId ratePlanId,
        DateOnly occupiedDate,
        Money price)
    {
        Id = id;
        RatePlanId = ratePlanId;
        OccupiedDate = occupiedDate;
        Price = price;
    }

    /// <summary>
    /// Creates the price of one RatePlan for one occupied date.
    /// </summary>
    /// <param name="id">The Pricing-owned DailyRate identifier.</param>
    /// <param name="ratePlan">The RatePlan whose identifier and selling currency apply.</param>
    /// <param name="occupiedDate">The specific inventory-consuming date being priced.</param>
    /// <param name="price">The non-negative price in the RatePlan's selling currency.</param>
    /// <returns>A DailyRate associated with the supplied RatePlan and occupied date.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when a required value is missing or the price uses another currency.
    /// </exception>
    public static DailyRate Create(
        DailyRateId id,
        RatePlan ratePlan,
        DateOnly occupiedDate,
        Money price)
    {
        if (id == default)
        {
            throw new DomainArgumentException("Daily rate ID is required.", nameof(id));
        }

        if (ratePlan is null)
        {
            throw new DomainArgumentException("Rate plan is required.", nameof(ratePlan));
        }

        if (occupiedDate == default)
        {
            throw new DomainArgumentException("Occupied date is required.", nameof(occupiedDate));
        }

        ValidatePrice(price, ratePlan.SellingCurrency);

        return new DailyRate(
            id,
            ratePlan.Id,
            occupiedDate,
            price);
    }

    /// <summary>
    /// Replaces the amount while preserving this DailyRate's selling currency.
    /// </summary>
    /// <param name="price">The new non-negative price in the existing currency.</param>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the price is missing or uses another currency.
    /// </exception>
    public void ChangePrice(Money price)
    {
        ValidatePrice(price, Price.Currency);

        Price = price;
    }

    /// <summary>
    /// Ensures a DailyRate price is present and uses its RatePlan selling currency.
    /// </summary>
    /// <param name="price">The price being assigned to the DailyRate.</param>
    /// <param name="expectedCurrency">The immutable selling currency to preserve.</param>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the price is missing or uses another currency.
    /// </exception>
    private static void ValidatePrice(Money price, Currency expectedCurrency)
    {
        if (price is null)
        {
            throw new DomainArgumentException("Daily rate price is required.", nameof(price));
        }

        if (price.Currency != expectedCurrency)
        {
            throw new DomainArgumentException(
                "Daily rate price must use the RatePlan selling currency.",
                nameof(price));
        }
    }
}
