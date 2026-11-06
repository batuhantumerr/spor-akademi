namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class AggregateRootTests
{
    [Fact]
    public void DomainEvents_NewAggregate_IsEmpty()
    {
        var aggregate = new TestAggregate(TestId.From(Guid.CreateVersion7()));

        aggregate.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void Raise_AddsEventsInOrder()
    {
        var aggregate = new TestAggregate(TestId.From(Guid.CreateVersion7()));

        aggregate.DoSomething();
        aggregate.DoSomething();

        aggregate.DomainEvents.Count.ShouldBe(2);
        aggregate.DomainEvents.ShouldAllBe(e => e.Equals(new TestEvent(aggregate.Id)));
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        var aggregate = new TestAggregate(TestId.From(Guid.CreateVersion7()));
        aggregate.DoSomething();

        aggregate.ClearDomainEvents();

        aggregate.DomainEvents.ShouldBeEmpty();
    }

    [Fact]
    public void DomainEvents_CannotBeChangedFromOutside()
    {
        var aggregate = new TestAggregate(TestId.From(Guid.CreateVersion7()));
        aggregate.DoSomething();

        // The list is read-only: only the aggregate itself can raise events (ADR-0009).
        aggregate.DomainEvents.ShouldNotBeAssignableTo<List<Domain.SharedKernel.IDomainEvent>>();
    }
}
