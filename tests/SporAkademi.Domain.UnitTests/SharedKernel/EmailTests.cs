using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class EmailTests
{
    [Theory]
    [InlineData("ali.veli@example.com", "ali.veli@example.com")]
    [InlineData("Ali.Veli@Example.COM", "ali.veli@example.com")]
    [InlineData("  ali@example.com  ", "ali@example.com")]
    [InlineData("ali+spor@mail.example.com.tr", "ali+spor@mail.example.com.tr")]
    public void Create_ValidAddress_IsNormalized(string input, string expected)
    {
        var result = Email.Create(input);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("@example.com")]
    [InlineData("ali@")]
    [InlineData("ali@@example.com")]
    [InlineData("ali@veli@example.com")]
    [InlineData("ali@example")]
    [InlineData("ali@.example.com")]
    [InlineData("ali@example.com.")]
    [InlineData("ali@example..com")]
    [InlineData("ali veli@example.com")]
    [InlineData("ali@exam ple.com")]
    public void Create_InvalidFormat_ReturnsEmailInvalid(string input)
    {
        Email.Create(input).Error.ShouldBe(ValueObjectErrors.EmailInvalid);
    }

    [Fact]
    public void Create_LocalPartLongerThan64_ReturnsEmailInvalid()
    {
        Email.Create(new string('a', 64) + "@example.com").IsSuccess.ShouldBeTrue();
        Email.Create(new string('a', 65) + "@example.com").Error.ShouldBe(ValueObjectErrors.EmailInvalid);
    }

    [Fact]
    public void Create_LongerThan254_ReturnsEmailInvalid()
    {
        // "a@" + domain: 2 + 240 + 12 = 254 characters.
        var domain = new string('b', 240) + ".example.com";

        Email.Create("a@" + domain).IsSuccess.ShouldBeTrue();
        Email.Create("ab@" + domain).Error.ShouldBe(ValueObjectErrors.EmailInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Empty_ReturnsFieldRequired(string input)
    {
        Email.Create(input).Error.ShouldBe(ValueObjectErrors.FieldRequired);
    }

    [Fact]
    public void Create_Null_Throws()
    {
        // Null is a programming error (ADR-0007); the API turns a missing field into Field.Required first.
        Should.Throw<ArgumentNullException>(() => Email.Create(null!));
    }

    [Fact]
    public void Equals_SameAddressInDifferentCase_IsTrue()
    {
        Email.Create("Ali@Example.com").Value.ShouldBe(Email.Create("ali@example.com").Value);
    }
}
