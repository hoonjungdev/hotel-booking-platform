using System.Collections.ObjectModel;
using HotelBooking.Modules.Booking.Domain.References;
using HotelBooking.SharedKernel.Exceptions;
using HotelBooking.SharedKernel.ValueObjects;

namespace HotelBooking.Modules.Booking.Domain.Reservations.ValueObjects;

/// <summary>Preserves the complete price and terms agreed when a Reservation is created.</summary>
public sealed class ReservationPriceSnapshot : IEquatable<ReservationPriceSnapshot>
{
    private readonly ReadOnlyCollection<ReservationNightlyPrice> _nightlyPrices;

    private ReservationPriceSnapshot(
        PriceQuoteId priceQuoteId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        RatePlanId ratePlanId,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        ReservationNightlyPrice[] nightlyPrices,
        Money totalPrice,
        CancellationPolicySnapshot cancellationPolicy)
    {
        PriceQuoteId = priceQuoteId;
        HotelId = hotelId;
        RoomTypeId = roomTypeId;
        RatePlanId = ratePlanId;
        StayDateRange = stayDateRange;
        RequestedOccupancy = requestedOccupancy;
        _nightlyPrices = Array.AsReadOnly(nightlyPrices);
        TotalPrice = totalPrice;
        CancellationPolicy = cancellationPolicy;
    }

    /// <summary>Gets the accepted Price Quote identifier.</summary>
    public PriceQuoteId PriceQuoteId { get; }

    /// <summary>Gets the Hotel that agreed the Reservation price.</summary>
    public HotelId HotelId { get; }

    /// <summary>Gets the Room Type priced for the Reservation.</summary>
    public RoomTypeId RoomTypeId { get; }

    /// <summary>Gets the Rate Plan whose agreed terms were accepted.</summary>
    public RatePlanId RatePlanId { get; }

    /// <summary>Gets the stay covered by the agreed nightly prices.</summary>
    public StayDateRange StayDateRange { get; }

    /// <summary>Gets the Guest composition agreed for the reserved room.</summary>
    public RequestedOccupancy RequestedOccupancy { get; }

    /// <summary>Gets immutable nightly prices ordered by occupied date.</summary>
    public IReadOnlyList<ReservationNightlyPrice> NightlyPrices => _nightlyPrices;

    /// <summary>Gets the calculated sum of every agreed nightly price.</summary>
    public Money TotalPrice { get; }

    /// <summary>Gets the immutable Cancellation Policy accepted with the price.</summary>
    public CancellationPolicySnapshot CancellationPolicy { get; }

    /// <summary>Creates a complete Booking-owned snapshot from an accepted Price Quote.</summary>
    /// <exception cref="DomainArgumentException">
    /// Thrown when an identifier or required value is missing.
    /// </exception>
    /// <exception cref="DomainException">
    /// Thrown when nightly prices do not exactly and consistently cover the stay.
    /// </exception>
    /// <exception cref="OverflowException">
    /// Thrown when the nightly price sum exceeds the range supported by <see cref="decimal"/>.
    /// </exception>
    public static ReservationPriceSnapshot Create(
        PriceQuoteId priceQuoteId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        RatePlanId ratePlanId,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        IReadOnlyCollection<ReservationNightlyPrice> nightlyPrices,
        CancellationPolicySnapshot cancellationPolicy)
    {
        ValidateRequiredValues(
            priceQuoteId,
            hotelId,
            roomTypeId,
            ratePlanId,
            stayDateRange,
            requestedOccupancy,
            nightlyPrices,
            cancellationPolicy);

        ValidateNightlyPrices(stayDateRange, nightlyPrices);

        ReservationNightlyPrice[] orderedNightlyPrices = nightlyPrices
            .OrderBy(nightlyPrice => nightlyPrice.OccupiedDate)
            .ToArray();

        Money totalPrice = orderedNightlyPrices.Aggregate(
            Money.Zero(orderedNightlyPrices[0].Price.Currency),
            (total, nightlyPrice) => total.Add(nightlyPrice.Price));

        return new ReservationPriceSnapshot(
            priceQuoteId,
            hotelId,
            roomTypeId,
            ratePlanId,
            stayDateRange,
            requestedOccupancy,
            orderedNightlyPrices,
            totalPrice,
            cancellationPolicy);
    }

    /// <inheritdoc />
    public bool Equals(ReservationPriceSnapshot? other)
    {
        return ReferenceEquals(this, other)
            || other is not null
            && PriceQuoteId == other.PriceQuoteId
            && HotelId == other.HotelId
            && RoomTypeId == other.RoomTypeId
            && RatePlanId == other.RatePlanId
            && StayDateRange == other.StayDateRange
            && RequestedOccupancy == other.RequestedOccupancy
            && _nightlyPrices.SequenceEqual(other._nightlyPrices)
            && TotalPrice == other.TotalPrice
            && CancellationPolicy.Equals(other.CancellationPolicy);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is ReservationPriceSnapshot other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(PriceQuoteId);
        hash.Add(HotelId);
        hash.Add(RoomTypeId);
        hash.Add(RatePlanId);
        hash.Add(StayDateRange);
        hash.Add(RequestedOccupancy);

        foreach (ReservationNightlyPrice nightlyPrice in _nightlyPrices)
        {
            hash.Add(nightlyPrice);
        }

        hash.Add(TotalPrice);
        hash.Add(CancellationPolicy);

        return hash.ToHashCode();
    }

    private static void ValidateRequiredValues(
        PriceQuoteId priceQuoteId,
        HotelId hotelId,
        RoomTypeId roomTypeId,
        RatePlanId ratePlanId,
        StayDateRange stayDateRange,
        RequestedOccupancy requestedOccupancy,
        IReadOnlyCollection<ReservationNightlyPrice> nightlyPrices,
        CancellationPolicySnapshot cancellationPolicy)
    {
        if (priceQuoteId == default)
        {
            throw new DomainArgumentException("Price Quote ID is required.", nameof(priceQuoteId));
        }

        if (hotelId == default)
        {
            throw new DomainArgumentException("Hotel ID is required.", nameof(hotelId));
        }

        if (roomTypeId == default)
        {
            throw new DomainArgumentException("Room Type ID is required.", nameof(roomTypeId));
        }

        if (ratePlanId == default)
        {
            throw new DomainArgumentException("Rate Plan ID is required.", nameof(ratePlanId));
        }

        if (stayDateRange is null)
        {
            throw new DomainArgumentException("Stay Date Range is required.", nameof(stayDateRange));
        }

        if (requestedOccupancy is null)
        {
            throw new DomainArgumentException(
                "Requested Occupancy is required.",
                nameof(requestedOccupancy));
        }

        if (nightlyPrices is null || nightlyPrices.Count == 0)
        {
            throw new DomainArgumentException(
                "Reservation nightly prices are required.",
                nameof(nightlyPrices));
        }

        if (nightlyPrices.Any(nightlyPrice => nightlyPrice is null))
        {
            throw new DomainArgumentException(
                "Reservation nightly prices cannot contain a missing price.",
                nameof(nightlyPrices));
        }

        if (cancellationPolicy is null)
        {
            throw new DomainArgumentException(
                "Cancellation Policy snapshot is required.",
                nameof(cancellationPolicy));
        }
    }

    private static void ValidateNightlyPrices(
        StayDateRange stayDateRange,
        IReadOnlyCollection<ReservationNightlyPrice> nightlyPrices)
    {
        if (nightlyPrices
            .GroupBy(nightlyPrice => nightlyPrice.OccupiedDate)
            .Any(group => group.Count() > 1))
        {
            throw new DomainException(
                "Reservation nightly prices cannot contain duplicate occupied dates.");
        }

        HashSet<DateOnly> expectedOccupiedDates = stayDateRange.OccupiedDates.ToHashSet();
        HashSet<DateOnly> actualOccupiedDates = nightlyPrices
            .Select(nightlyPrice => nightlyPrice.OccupiedDate)
            .ToHashSet();

        if (!expectedOccupiedDates.SetEquals(actualOccupiedDates))
        {
            throw new DomainException(
                "Reservation nightly prices must cover every occupied stay date exactly once.");
        }

        if (nightlyPrices
            .Select(nightlyPrice => nightlyPrice.Price.Currency)
            .Distinct()
            .Count() != 1)
        {
            throw new DomainException(
                "Reservation nightly prices must use one Hotel Selling Currency.");
        }
    }
}
