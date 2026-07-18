using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Pricing.Domain.References;

/// <summary>
/// Carries a hotel's identity and immutable selling currency together inside Pricing.
/// </summary>
public sealed record HotelRateSettings
{
    private HotelRateSettings(HotelId hotelId, Currency sellingCurrency)
    {
        HotelId = hotelId;
        SellingCurrency = sellingCurrency;
    }

    /// <summary>Gets the hotel whose rates use these settings.</summary>
    public HotelId HotelId { get; }

    /// <summary>Gets the hotel's selling currency supplied to Pricing.</summary>
    public Currency SellingCurrency { get; }

    /// <summary>
    /// Creates Pricing's immutable view of the hotel facts required to define rates.
    /// </summary>
    /// <param name="hotelId">The existing hotel identifier owned by Property.</param>
    /// <param name="sellingCurrency">The hotel's selling currency obtained at the module boundary.</param>
    /// <returns>The hotel identifier and selling currency as one indivisible value.</returns>
    /// <exception cref="DomainArgumentException">
    /// Thrown when the hotel identifier or selling currency is missing.
    /// </exception>
    internal static HotelRateSettings Create(
        HotelId hotelId,
        Currency sellingCurrency)
    {
        if (hotelId == default)
        {
            throw new DomainArgumentException("Hotel ID is required.", nameof(hotelId));
        }

        if (sellingCurrency is null)
        {
            throw new DomainArgumentException(
                "Hotel selling currency is required.",
                nameof(sellingCurrency));
        }

        return new HotelRateSettings(hotelId, sellingCurrency);
    }
}
