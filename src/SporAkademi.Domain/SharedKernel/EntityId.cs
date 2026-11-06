namespace SporAkademi.Domain.SharedKernel;

public static class EntityId
{
    // UUID v7 starts with a timestamp, so new rows land at the end of the index (domain model §8).
    // The timestamp is only used for ordering, not for business rules; that is why this does not
    // go through TimeProvider (CLAUDE.md).
    public static TId New<TId>()
        where TId : struct, IEntityId<TId> => TId.From(Guid.CreateVersion7());
}
