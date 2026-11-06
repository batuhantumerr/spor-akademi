using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("+905321234567", "+905321234567")]
    [InlineData("+90 532 123 45 67", "+905321234567")]
    [InlineData("0532 123 45 67", "+905321234567")]
    [InlineData("05321234567", "+905321234567")]
    [InlineData("5321234567", "+905321234567")]
    [InlineData("(0532) 123-45-67", "+905321234567")]
    [InlineData("  0532 123 45 67  ", "+905321234567")]
    [InlineData("0212 555 12 34", "+902125551234")]
    [InlineData("0312 555 12 34", "+903125551234")]
    [InlineData("0462 555 12 34", "+904625551234")]
    public void Create_TurkishNumber_IsNormalizedToE164(string input, string expected)
    {
        var result = PhoneNumber.Create(input);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Value.ShouldBe(expected);
    }

    [Theory]
    [InlineData("532123456")] // 9 digits
    [InlineData("53212345678")] // 11 digits without a leading 0
    [InlineData("053212345678")] // 12 digits with a leading 0
    [InlineData("+9053212345678")] // too long after +90
    [InlineData("08501234567")] // 850 numbers are not accepted (first digit 8)
    [InlineData("06321234567")] // first digit 6
    [InlineData("01321234567")] // first digit 1
    [InlineData("+15551234567")] // not Turkey
    [InlineData("+90 0532 123 45 67")] // trunk 0 after the country code
    [InlineData("0532.123.45.67")] // dots are not a separator
    [InlineData("0532 abc 45 67")]
    [InlineData("٠٥٣٢١٢٣٤٥٦٧")] // non-ASCII digits
    [InlineData("+")]
    public void Create_InvalidNumber_ReturnsPhoneNumberInvalid(string input)
    {
        PhoneNumber.Create(input).Error.ShouldBe(ValueObjectErrors.PhoneNumberInvalid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_Empty_ReturnsFieldRequired(string input)
    {
        PhoneNumber.Create(input).Error.ShouldBe(ValueObjectErrors.FieldRequired);
    }

    [Fact]
    public void Create_Null_Throws()
    {
        Should.Throw<ArgumentNullException>(() => PhoneNumber.Create(null!));
    }

    [Fact]
    public void Equals_SameNumberWrittenDifferently_IsTrue()
    {
        // Normalizing makes the same number equal however it was typed; uniqueness checks rely on this (US-13).
        PhoneNumber.Create("0532 123 45 67").Value.ShouldBe(PhoneNumber.Create("+905321234567").Value);
    }
}
