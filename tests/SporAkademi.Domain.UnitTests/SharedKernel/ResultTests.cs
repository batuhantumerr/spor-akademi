using SporAkademi.Domain.SharedKernel;

namespace SporAkademi.Domain.UnitTests.SharedKernel;

public sealed class ResultTests
{
    private static readonly Error _error = Error.BusinessRule("Test.Failed", "The test operation failed.");

    [Fact]
    public void Success_IsSuccessAndHasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        Should.Throw<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void Failure_IsFailureAndHasTheError()
    {
        var result = Result.Failure(_error);

        result.IsSuccess.ShouldBeFalse();
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(_error);
    }

    [Fact]
    public void Failure_NullError_Throws()
    {
        // A failure always has an error; there is no "no error" value to fail with (ADR-0007).
        Should.Throw<ArgumentNullException>(() => Result.Failure(null!));
        Should.Throw<ArgumentNullException>(() => Result.Failure<int>(null!));
    }

    [Fact]
    public void SuccessWithValue_HasTheValue()
    {
        var result = Result.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
        Should.Throw<InvalidOperationException>(() => result.Error);
    }

    [Fact]
    public void SuccessWithValue_NullValue_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Result.Success<string>(null!));
    }

    [Fact]
    public void FailureWithValueType_HasTheErrorAndNoValue()
    {
        var result = Result.Failure<int>(_error);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(_error);
        // Reading the value of a failure is a programming error, not an expected failure (ADR-0007).
        Should.Throw<InvalidOperationException>(() => result.Value);
    }
}
