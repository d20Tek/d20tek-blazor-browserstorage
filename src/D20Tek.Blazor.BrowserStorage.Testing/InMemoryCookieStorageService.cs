namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// In-memory implementation of <see cref="ICookieStorageService"/> for tests. Also implements
/// <see cref="IInMemoryStorage"/> for seeding and inspection. Can be used directly from any
/// test framework (bUnit, MSTest, xUnit, NUnit) without additional adapters.
/// </summary>
/// <remarks>
/// This fake does not model cookie-specific attributes (path, domain, SameSite, Secure,
/// expiration); it only mirrors the key/value and change-notification semantics shared with
/// <see cref="ILocalStorageService"/> and <see cref="ISessionStorageService"/>.
/// </remarks>
/// <param name="options">The browser storage options.</param>
public sealed class InMemoryCookieStorageService(IOptions<BrowserStorageOptions> options)
    : InMemoryWebStorageService("cookieStorage", options), ICookieStorageService
{
    /// <summary>Initializes a new instance with default <see cref="BrowserStorageOptions"/>.</summary>
    public InMemoryCookieStorageService() : this(new BrowserStorageOptions()) { }

    /// <summary>Initializes a new instance with the specified <see cref="BrowserStorageOptions"/>.</summary>
    /// <param name="options">The browser storage options.</param>
    public InMemoryCookieStorageService(BrowserStorageOptions options) : this(Options.Create(options)) { }
}
