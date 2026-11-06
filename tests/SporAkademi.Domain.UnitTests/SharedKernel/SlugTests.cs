using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class SlugTests
{
    [Theory]
    [InlineData("abc")]
    [InlineData("spor-akademi")]
    [InlineData("a1-b2-c3")]
    [InlineData("123")]
    public void Create_ValidSlug_KeepsTheValue(string input)
    {
        var result = Slug.Create(input);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(input);
    }

    [Fact]
    public void Create_LengthLimits_AreThreeToFifty()
    {
        Slug.Create("ab").Error.ShouldBe(ValueObjectErrors.SlugInvalid);
        Slug.Create(new string('a', 50)).IsSuccess.ShouldBeTrue();
        Slug.Create(new string('a', 51)).Error.ShouldBe(ValueObjectErrors.SlugInvalid);
    }

    [Theory]
    [InlineData("Spor")] // upper case is not lowered silently
    [InlineData("spor_akademi")]
    [InlineData("spor akademi")]
    [InlineData(" spor")]
    [InlineData("-spor")]
    [InlineData("spor-")]
    [InlineData("spor--akademi")]
    [InlineData("şampiyon")] // only a-z
    [InlineData("spor\n")] // a trailing newline must not slip past the end anchor
    public void Create_InvalidSlug_ReturnsSlugInvalid(string input)
    {
        Slug.Create(input).Error.ShouldBe(ValueObjectErrors.SlugInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Empty_ReturnsFieldRequired(string input)
    {
        Slug.Create(input).Error.ShouldBe(ValueObjectErrors.FieldRequired);
    }

    [Fact]
    public void Create_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Slug.Create(null!));
    }
}
