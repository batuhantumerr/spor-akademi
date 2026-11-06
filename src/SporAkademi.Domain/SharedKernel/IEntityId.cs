namespace SporAkademi.Domain.SharedKernel;

public interface IEntityId<TSelf>
    where TSelf : struct, IEntityId<TSelf>
{
    Guid Value { get; }

    static abstract TSelf From(Guid value);
}
