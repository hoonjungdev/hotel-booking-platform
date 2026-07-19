using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Pricing.Domain.PriceQuotes.ValueObjects;

/// <summary>Preserves the price offered for one occupied night in a PriceQuote.</summary>
public sealed record NightlyPrice
{
    private NightlyPrice(DateOnly occupiedDate, Money price)
    {
        OccupiedDate = occupiedDate;
        Price = price;
    }

    /// <summary>Gets the occupied date priced by this quote line.</summary>
    public DateOnly OccupiedDate { get; }

    /// <summary>Gets the immutable price offered for the occupied date.</summary>
    public Money Price { get; }

    /// <summary>Creates a quote-owned nightly price from a validated DailyRate.</summary>
    /// <param name="occupiedDate">The occupied date included in the quote.</param>
    /// <param name="price">The immutable quoted price for that date.</param>
    /// <returns>A nightly price snapshot.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the occupied date or price is missing.
    /// </exception>
    internal static NightlyPrice Create(DateOnly occupiedDate, Money price)
    {
        if (occupiedDate == default)
        {
            throw new DomainArgumentException(
                "Nightly price occupied date is required.",
                nameof(occupiedDate));
        }

        if (price is null)
        {
            throw new DomainArgumentException(
                "Nightly price is required.",
                nameof(price));
        }

        return new NightlyPrice(occupiedDate, price);
    }
}
