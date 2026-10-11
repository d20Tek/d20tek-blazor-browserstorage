using CookieConsent.Models;
using D20Tek.Blazor.BrowserStorage;

namespace CookieConsent.Services;

/// <summary>
/// Wraps <see cref="ICookieStorageService"/> to manage the visitor's cookie-consent decision.
/// Demonstrates a realistic use case for cookies (as opposed to localStorage/sessionStorage):
/// the decision is long-lived (1 year, configured in Program.cs), scoped to the whole site via
/// the cookie's path, and could be read by a server-rendered page on the same domain.
/// </summary>
public class ConsentBannerService(ICookieStorageService cookies)
{
    private const string ConsentKey = "cookie-consent";
    private readonly ICookieStorageService _cookies = cookies;

    /// <summary>
    /// The current consent decision, or null when the visitor has not decided yet.
    /// </summary>
    public ConsentPreferences? Current { get; private set; }

    /// <summary>
    /// True once the visitor has made (and persisted) a consent decision.
    /// </summary>
    public bool HasDecided => Current is not null;

    /// <summary>
    /// Raised whenever the consent decision changes, so the banner and settings page can stay
    /// in sync without manually polling the cookie.
    /// </summary>
    public event Action? OnChanged;

    /// <summary>
    /// Loads any previously persisted consent decision. Call once during app/component startup.
    /// </summary>
    public async Task LoadAsync()
    {
        var result = await _cookies.GetAsync<ConsentPreferences>(ConsentKey);
        Current = result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Accepts all non-essential cookie categories.
    /// </summary>
    public Task AcceptAllAsync() => SaveAsync(ConsentPreferences.AllowAll());

    /// <summary>
    /// Rejects all non-essential cookie categories.
    /// </summary>
    public Task RejectNonEssentialAsync() => SaveAsync(ConsentPreferences.RejectAll());

    /// <summary>
    /// Saves a custom combination of non-essential cookie categories.
    /// </summary>
    public Task SaveCustomPreferencesAsync(bool analytics, bool marketing) => SaveAsync(new ConsentPreferences
    {
        Analytics = analytics,
        Marketing = marketing,
        DecidedAtUtc = DateTime.UtcNow
    });

    /// <summary>
    /// Clears the consent decision, causing the banner to reappear on next render.
    /// </summary>
    public async Task ResetConsentAsync()
    {
        var result = await _cookies.RemoveAsync(ConsentKey);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"[ConsentBannerService] Failed to reset consent: {result.ErrorMessage}");
        }

        Current = null;
        OnChanged?.Invoke();
    }

    private async Task SaveAsync(ConsentPreferences preferences)
    {
        var result = await _cookies.SetAsync(ConsentKey, preferences);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"[ConsentBannerService] Failed to save consent: {result.ErrorMessage}");
        }

        Current = preferences;
        OnChanged?.Invoke();
    }
}
