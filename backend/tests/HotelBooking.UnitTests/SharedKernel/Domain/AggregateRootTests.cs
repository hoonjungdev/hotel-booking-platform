using HotelBooking.SharedKernel.Domain;

namespace HotelBooking.UnitTests.SharedKernel.Domain;

public class AggregateRootTests
{
    [Fact]
    public void RaiseDomainEvent_adds_event()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        var domainEvent = new TestDomainEvent(DateTimeOffset.UtcNow);

        aggregate.Raise(domainEvent);

        IDomainEvent raisedDomainEvent = Assert.Single(aggregate.DomainEvents);
        Assert.Same(domainEvent, raisedDomainEvent);
    }

    [Fact]
    public void ClearDomainEvents_removes_all_events()
    {
        var aggregate = new TestAggregate(Guid.NewGuid());
        aggregate.Raise(new TestDomainEvent(DateTimeOffset.UtcNow));

        aggregate.ClearDomainEvents();

        Assert.Empty(aggregate.DomainEvents);
    }

    private sealed class TestAggregate : AggregateRoot<Guid>
    {
        public TestAggregate(Guid id)
        {
            Id = id;
        }

        public void Raise(IDomainEvent domainEvent)
        {
            RaiseDomainEvent(domainEvent);
        }
    }

    private sealed record TestDomainEvent(
        DateTimeOffset OccurredAt) : IDomainEvent;
}
