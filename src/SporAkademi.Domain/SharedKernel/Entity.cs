namespace SporAkademi.Domain.SharedKernel;

// An entity is identified by its Id, not by its field values: two objects of the same entity
// type with the same Id are the same entity.
public abstract class Entity<TId>(TId id)
    where TId : struct, IEntityId<TId>
{
    public TId Id { get; } = id;

    public override bool Equals(object? obj) =>
        obj is Entity<TId> other && GetType() == other.GetType() && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);
}
