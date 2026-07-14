namespace HotelBooking.SharedKernel.Domain;

/// <summary>
/// Defines an aggregate root that records domain events raised by its behavior.
/// </summary>
/// <typeparam name="TId">The strongly typed identifier used by the aggregate.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the domain events raised since the aggregate was loaded or last cleared.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Records a domain event produced by a successful aggregate transition.
    /// </summary>
    /// <param name="domainEvent">The event that describes the completed domain change.</param>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes domain events after the application layer has processed them.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
