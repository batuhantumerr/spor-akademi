namespace SporAkademi.Domain.SharedKernel;

// A Turkish phone number stored in E.164 form (+90 and ten digits), so the same number typed in
// different ways is always equal (domain model §7). Only Turkish landline and mobile numbers are
// accepted: the national number starts with 2-5. Foreign numbers are out of scope for now.
public sealed record PhoneNumber
{
    private const string CountryCode = "+90";
    private const int NationalNumberLength = 10;

    private PhoneNumber(string value) => Value = value;

    public string Value { get; }

    public static Result<PhoneNumber> Create(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var trimmed = value.Trim();

        if (trimmed.Length == 0)
        {
            return Result.Failure<PhoneNumber>(ValueObjectErrors.FieldRequired);
        }

        // Spaces, hyphens and parentheses are only formatting; everything else is kept and checked.
        var compact = new string([.. trimmed.Where(c => c is not (' ' or '-' or '(' or ')'))]);

        var national =
            compact.StartsWith(CountryCode, StringComparison.Ordinal) ? compact[CountryCode.Length..]
            : compact.StartsWith('0') ? compact[1..]
            : compact;

        return IsValidNationalNumber(national)
            ? Result.Success(new PhoneNumber(CountryCode + national))
            : Result.Failure<PhoneNumber>(ValueObjectErrors.PhoneNumberInvalid);
    }

    private static bool IsValidNationalNumber(string national) =>
        national.Length == NationalNumberLength
        && national.All(char.IsAsciiDigit)
        && national[0] is >= '2' and <= '5';
}
