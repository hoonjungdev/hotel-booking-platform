using HotelBooking.SharedKernel.Domain;

namespace HotelBooking.Modules.Property.Domain.Hotels.Events;

/// <summary>
/// Records that a hotel completed review and became active.
/// </summary>
/// <param name="HotelId">The published hotel identifier.</param>
/// <param name="OccurredAt">The explicit publication timestamp.</param>
public sealed record HotelPublishedDomainEvent(
    HotelId HotelId,
    DateTimeOffset OccurredAt) : IDomainEvent;
