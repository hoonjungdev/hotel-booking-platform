using HotelBooking.SharedKernel.Domain;

namespace HotelBooking.Modules.Property.Domain.Hotels.Events;

public sealed record HotelPublishedDomainEvent(
    HotelId HotelId,
    DateTimeOffset OccurredAt) : IDomainEvent;
