namespace CookieConsent.Models;

/// <summary>
/// The cookie-consent decision recorded for the current visitor, serialized as the
/// payload of the "cookie-consent" cookie managed by <see cref="Services.ConsentBannerService"/>.
/// </summary>
public class ConsentPreferences
{
    /// <summary>
    /// Whether analytics cookies/scripts are allowed.
    /// </summary>
    public bool Analytics { get; set; }

    /// <summary>
    /// Whether marketing cookies/scripts are allowed.
    /// </summary>
    public bool Marketing { get; set; }

    /// <summary>
    /// The UTC timestamp when the visitor made this consent decision.
    /// </summary>
    public DateTime DecidedAtUtc { get; set; }

    /// <summary>
    /// A consent record with every non-essential category allowed.
    /// </summary>
    public static ConsentPreferences AllowAll() => new()
    {
        Analytics = true,
        Marketing = true,
        DecidedAtUtc = DateTime.UtcNow
    };

    /// <summary>
    /// A consent record with every non-essential category rejected.
    /// </summary>
    public static ConsentPreferences RejectAll() => new()
    {
        Analytics = false,
        Marketing = false,
        DecidedAtUtc = DateTime.UtcNow
    };
}
