using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

// Example types for testing the building blocks; real IDs and aggregates arrive with their modules.

public readonly record struct TestId(Guid Value) : IEntityId<TestId>
{
    public static TestId From(Guid value) => new(value);
}

public readonly record struct OtherTestId(Guid Value) : IEntityId<OtherTestId>
{
    public static OtherTestId From(Guid value) => new(value);
}

public sealed class TestEntity(TestId id) : Entity<TestId>(id);

public sealed class OtherTestEntity(TestId id) : Entity<TestId>(id);

public sealed record TestEvent(TestId AggregateId) : IDomainEvent;

public sealed class TestAggregate(TestId id) : AggregateRoot<TestId>(id)
{
    public void DoSomething() => Raise(new TestEvent(Id));
}
