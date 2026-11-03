using System.Globalization;

namespace SporAkademi.Api;

/// <summary>
/// Checks at startup that the operating system provides the time zone and culture data the application relies
/// on: school time zones are IANA ids (default Europe/Istanbul) and formats follow tr-TR (NFR-CMP-03).
/// Container images without tzdata or ICU (for example the default chiseled .NET images) would otherwise
/// fail or, worse, silently fall back to invariant behaviour in production.
/// </summary>
internal static class PlatformRequirements
{
    private const string TimeZoneId = "Europe/Istanbul";
    private const string CultureName = "tr-TR";

    public static void EnsureAvailable()
    {
        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }
        catch (TimeZoneNotFoundException exception)
        {
            throw new InvalidOperationException(
                $"Time zone '{TimeZoneId}' is not available. The operating system needs tzdata; " +
                "use the '-extra' variant of the chiseled .NET images.",
                exception);
        }

        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(CultureName);
        }
        catch (CultureNotFoundException exception)
        {
            throw new InvalidOperationException(
                $"Culture '{CultureName}' is not available. The operating system needs ICU; " +
                "use the '-extra' variant of the chiseled .NET images.",
                exception);
        }

        // In globalization-invariant mode a culture object can still be created, but it behaves like the
        // invariant culture; Turkish casing shows whether ICU data is really loaded.
        if (!string.Equals("i".ToUpper(culture), "İ", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Culture '{CultureName}' has no Turkish casing rules; globalization data (ICU) is missing. " +
                "Use the '-extra' variant of the chiseled .NET images.");
        }
    }
}
