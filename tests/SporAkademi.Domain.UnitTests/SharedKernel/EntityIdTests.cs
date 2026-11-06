using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class EntityIdTests
{
    [Fact]
    public void New_CreatesUuidVersion7()
    {
        var id = EntityId.New<TestId>();

        id.Value.Version.ShouldBe(7);
    }

    [Fact]
    public void New_CreatesDifferentIdsEachTime()
    {
        var first = EntityId.New<TestId>();
        var second = EntityId.New<TestId>();

        first.ShouldNotBe(second);
    }

    [Fact]
    public void New_LaterIdSortsAfterEarlierId()
    {
        // UUID v7 starts with a millisecond timestamp, so IDs created later sort later (domain model §8).
        var first = EntityId.New<TestId>();
        Thread.Sleep(2);
        var second = EntityId.New<TestId>();

        first.Value.CompareTo(second.Value).ShouldBeLessThan(0);
    }

    [Fact]
    public void From_KeepsTheValue()
    {
        var value = Guid.CreateVersion7();

        TestId.From(value).Value.ShouldBe(value);
    }
}
