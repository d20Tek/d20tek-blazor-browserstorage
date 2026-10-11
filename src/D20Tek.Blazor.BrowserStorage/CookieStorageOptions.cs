namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// The SameSite attribute values supported for cookies written via <see cref="ICookieStorageService"/>.
/// </summary>
public enum CookieSameSiteMode
{
    /// <summary>Cookies are sent with same-site requests and top-level navigation GET requests. Default.</summary>
    Lax,

    /// <summary>Cookies are sent with every request, including cross-site requests. Requires <see cref="CookieStorageOptions.Secure"/>.</summary>
    None,

    /// <summary>Cookies are only sent with same-site requests.</summary>
    Strict
}

/// <summary>
/// Configuration options for the cookie storage service, controlling the attributes applied to
/// cookies written by <see cref="ICookieStorageService"/>.
/// </summary>
public sealed class CookieStorageOptions
{
    /// <summary>
    /// The default lifetime applied to cookies when a value is set without an explicit expiration.
    /// When null, the cookie is a session cookie and is removed when the browser session ends.
    /// Default: 30 days.
    /// </summary>
    public TimeSpan? DefaultExpiration { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// The path attribute applied to cookies. Default: "/".
    /// </summary>
    public string Path { get; set; } = "/";

    /// <summary>
    /// The domain attribute applied to cookies. Default: null (host-only cookie).
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// The SameSite attribute applied to cookies. Default: <see cref="CookieSameSiteMode.Lax"/>.
    /// </summary>
    public CookieSameSiteMode SameSite { get; set; } = CookieSameSiteMode.Lax;

    /// <summary>
    /// Whether the Secure attribute is applied to cookies, restricting them to HTTPS connections.
    /// Default: false. Must be true when <see cref="SameSite"/> is <see cref="CookieSameSiteMode.None"/>.
    /// </summary>
    public bool Secure { get; set; }
}
