using System.Text.RegularExpressions;

namespace SporAkademi.Domain.SharedKernel;

// A short, URL-safe name (domain model §7). It is not lowered or trimmed silently: the user sees the
// slug exactly as it is stored.
public sealed partial record Slug
{
    private const int MinLength = 3;
    private const int MaxLength = 50;

    private Slug(string value) => Value = value;

    public string Value { get; }

    public static Result<Slug> Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Slug>(ValueObjectErrors.FieldRequired);
        }

        return value.Length is >= MinLength and <= MaxLength && Pattern().IsMatch(value)
            ? Result.Success(new Slug(value))
            : Result.Failure<Slug>(ValueObjectErrors.SlugInvalid);
    }

    // \z instead of $: $ also matches before a trailing newline, so "spor\n" would pass.
    [GeneratedRegex(@"^[a-z0-9]+(-[a-z0-9]+)*\z", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
