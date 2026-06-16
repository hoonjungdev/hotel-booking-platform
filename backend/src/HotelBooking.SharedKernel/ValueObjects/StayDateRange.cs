namespace HotelBooking.SharedKernel.ValueObjects;

public sealed record StayDateRange
{
    public DateOnly CheckInDate { get; }
    public DateOnly CheckOutDate { get; }

    public StayDateRange(DateOnly checkInDate, DateOnly checkOutDate)
    {
        if (checkInDate >= checkOutDate)
        {
            throw new DomainException("Check-in date must be before check-out date");
        }

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    public int Nights => CheckOutDate.DayNumber - CheckInDate.DayNumber;
    public IReadOnlyList<DateOnly> OccupiedDates =>
        Enumerable
            .Range(0, Nights)
            .Select(CheckInDate.AddDays)
            .ToList();
}
