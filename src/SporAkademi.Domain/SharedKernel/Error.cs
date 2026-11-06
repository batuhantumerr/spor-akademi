using System.Diagnostics.CodeAnalysis;

namespace SporAkademi.Domain.SharedKernel;

// An expected failure (ADR-0007). Each module defines its errors in its own *Errors class and
// lists the codes in docs/api/error-codes.md; the type decides the HTTP status in the API.
[SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "DEC-096: the name comes from ADR-0007; the project is C# only and is not shipped as a library.")]
public sealed record Error
{
    private Error(string code, string description, ErrorType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static Error Validation(string code, string description) => new(code, description, ErrorType.Validation);

    public static Error Unauthorized(string code, string description) => new(code, description, ErrorType.Unauthorized);

    public static Error Forbidden(string code, string description) => new(code, description, ErrorType.Forbidden);

    public static Error NotFound(string code, string description) => new(code, description, ErrorType.NotFound);

    public static Error Conflict(string code, string description) => new(code, description, ErrorType.Conflict);

    public static Error BusinessRule(string code, string description) => new(code, description, ErrorType.BusinessRule);
}
