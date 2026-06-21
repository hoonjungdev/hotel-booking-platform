namespace HotelBooking.SharedKernel.Domain;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
