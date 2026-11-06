namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class EntityTests
{
    [Fact]
    public void Equals_SameTypeAndId_IsTrue()
    {
        var id = TestId.From(Guid.CreateVersion7());

        var first = new TestEntity(id);
        var second = new TestEntity(id);

        first.Equals(second).ShouldBeTrue();
        (first == second).ShouldBeTrue();
        (first != second).ShouldBeFalse();
        first.GetHashCode().ShouldBe(second.GetHashCode());
    }

    [Fact]
    public void Equals_DifferentId_IsFalse()
    {
        var first = new TestEntity(TestId.From(Guid.CreateVersion7()));
        var second = new TestEntity(TestId.From(Guid.CreateVersion7()));

        first.Equals(second).ShouldBeFalse();
        (first == second).ShouldBeFalse();
        (first != second).ShouldBeTrue();
    }

    [Fact]
    public void Equals_DifferentEntityTypeWithSameId_IsFalse()
    {
        var id = TestId.From(Guid.CreateVersion7());

        var entity = new TestEntity(id);
        var other = new OtherTestEntity(id);

        entity.Equals(other).ShouldBeFalse();
        (entity == other).ShouldBeFalse();
    }

    [Fact]
    public void Equals_Null_IsFalse()
    {
        var entity = new TestEntity(TestId.From(Guid.CreateVersion7()));

        entity.Equals(null).ShouldBeFalse();
        (entity == null).ShouldBeFalse();
        (null == entity).ShouldBeFalse();
    }
}
