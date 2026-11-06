namespace SporAkademi.Domain.SharedKernel;

// Field-level errors of the shared value objects. The codes and their Turkish messages are listed in
// docs/api/error-codes.md; the descriptions here are for developers, not for users.
public static class ValueObjectErrors
{
    public static readonly Error FieldRequired = Error.Validation("Field.Required", "The field is required.");

    public static readonly Error EmailInvalid = Error.Validation("Email.Invalid", "The email address is not valid.");

    public static readonly Error PhoneNumberInvalid =
        Error.Validation("PhoneNumber.Invalid", "The phone number is not a valid Turkish number.");

    public static readonly Error SlugInvalid =
        Error.Validation("Slug.Invalid", "The slug must be 3-50 characters of a-z, 0-9 and single hyphens.");
}
