using HotelBooking.SharedKernel.Exceptions;

namespace HotelBooking.SharedKernel.ValueObjects;

/// <summary>
/// Represents a hotel stay whose check-in date is included and check-out date is excluded.
/// </summary>
public sealed record StayDateRange
{
    /// <summary>
    /// Gets the first occupied date of the stay.
    /// </summary>
    public DateOnly CheckInDate { get; }

    /// <summary>
    /// Gets the departure date, which does not consume inventory.
    /// </summary>
    public DateOnly CheckOutDate { get; }

    /// <summary>
    /// Creates a valid stay date range.
    /// </summary>
    /// <param name="checkInDate">The included check-in date.</param>
    /// <param name="checkOutDate">The excluded check-out date.</param>
    /// <exception cref="DomainException">Thrown when check-in is not before check-out.</exception>
    public StayDateRange(DateOnly checkInDate, DateOnly checkOutDate)
    {
        if (checkInDate >= checkOutDate)
        {
            throw new DomainException("Check-in date must be before check-out date");
        }

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    /// <summary>
    /// Gets the number of occupied nights in the stay.
    /// </summary>
    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;

    /// <summary>
    /// Gets every inventory-consuming date from check-in through the day before check-out.
    /// </summary>
    public IReadOnlyList<DateOnly> OccupiedDates =>
        Enumerable
            .Range(0, Nights)
            .Select(CheckInDate.AddDays)
            .ToList();
}
