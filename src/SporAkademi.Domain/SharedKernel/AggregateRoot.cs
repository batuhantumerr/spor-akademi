namespace SporAkademi.Domain.SharedKernel;

// Only the aggregate raises its own events; the infrastructure reads and clears them when it
// saves, in the same transaction (ADR-0009).
public abstract class AggregateRoot<TId>(TId id) : Entity<TId>(id)
    where TId : struct, IEntityId<TId>
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void Raise(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);
        _domainEvents.Add(domainEvent);
    }
}
