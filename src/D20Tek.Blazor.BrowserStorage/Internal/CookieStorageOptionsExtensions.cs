namespace D20Tek.Blazor.BrowserStorage.Internal;

internal static class CookieStorageOptionsExtensions
{
    /// <summary>
    /// Converts a <see cref="CookieSameSiteMode"/> value to the attribute token expected by the
    /// <c>document.cookie</c> SameSite attribute.
    /// </summary>
    public static string ToSameSiteToken(this CookieStorageOptions options) => options.SameSite switch
    {
        CookieSameSiteMode.None => "None",
        CookieSameSiteMode.Strict => "Strict",
        _ => "Lax",
    };
}
