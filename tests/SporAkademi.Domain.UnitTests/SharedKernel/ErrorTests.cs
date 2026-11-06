using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class ErrorTests
{
    public static TheoryData<Func<string, string, Error>, ErrorType> Factories => new()
    {
        { Error.Validation, ErrorType.Validation },
        { Error.Unauthorized, ErrorType.Unauthorized },
        { Error.Forbidden, ErrorType.Forbidden },
        { Error.NotFound, ErrorType.NotFound },
        { Error.Conflict, ErrorType.Conflict },
        { Error.BusinessRule, ErrorType.BusinessRule },
    };

    [Theory]
    [MemberData(nameof(Factories))]
    public void Factory_CreatesErrorOfItsType(Func<string, string, Error> factory, ErrorType expectedType)
    {
        var error = factory("Test.Code", "Test description.");

        error.Code.ShouldBe("Test.Code");
        error.Description.ShouldBe("Test description.");
        error.Type.ShouldBe(expectedType);
    }

    [Fact]
    public void Equals_SameCodeDescriptionAndType_IsTrue()
    {
        Error.Conflict("Test.Code", "Test description.")
            .ShouldBe(Error.Conflict("Test.Code", "Test description."));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Factory_EmptyCode_Throws(string code)
    {
        Should.Throw<ArgumentException>(() => Error.Validation(code, "Test description."));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Factory_EmptyDescription_Throws(string description)
    {
        Should.Throw<ArgumentException>(() => Error.Validation("Test.Code", description));
    }
}
