namespace SporAkademi.Domain.SharedKernel;

// Stored trimmed and in lower case, so the same address is always equal to itself (domain model §7).
// The format check is deliberately simple; whether the address really exists is only known by sending
// it an email.
public sealed record Email
{
    private const int MaxLength = 254;
    private const int MaxLocalPartLength = 64;

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length == 0)
        {
            return Result.Failure<Email>(ValueObjectErrors.FieldRequired);
        }

        return IsValid(normalized)
            ? Result.Success(new Email(normalized))
            : Result.Failure<Email>(ValueObjectErrors.EmailInvalid);
    }

    private static bool IsValid(string address)
    {
        if (address.Length > MaxLength || address.Any(char.IsWhiteSpace))
        {
            return false;
        }

        var at = address.IndexOf('@', StringComparison.Ordinal);

        if (at <= 0 || at != address.LastIndexOf('@') || at > MaxLocalPartLength)
        {
            return false;
        }

        var domain = address[(at + 1)..];

        return domain.Contains('.', StringComparison.Ordinal)
            && !domain.StartsWith('.')
            && !domain.EndsWith('.')
            && !domain.Contains("..", StringComparison.Ordinal);
    }
}
