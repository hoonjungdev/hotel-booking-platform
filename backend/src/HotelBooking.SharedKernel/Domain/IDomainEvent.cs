namespace HotelBooking.SharedKernel.Domain;

/// <summary>
/// Describes a fact that occurred inside a domain aggregate.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the explicit timestamp at which the domain fact occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
