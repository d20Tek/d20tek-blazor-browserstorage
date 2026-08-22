namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// In-memory implementation of <see cref="ILocalStorageService"/> for tests. Also implements
/// <see cref="IInMemoryStorage"/> for seeding and inspection. Can be used directly from any
/// test framework (bUnit, MSTest, xUnit, NUnit) without additional adapters.
/// </summary>
/// <param name="options">The browser storage options.</param>
public sealed class InMemoryLocalStorageService(IOptions<BrowserStorageOptions> options)
    : InMemoryWebStorageService("localStorage", options), ILocalStorageService
{
    /// <summary>Initializes a new instance with default <see cref="BrowserStorageOptions"/>.</summary>
    public InMemoryLocalStorageService() : this(new BrowserStorageOptions()) { }

    /// <summary>Initializes a new instance with the specified <see cref="BrowserStorageOptions"/>.</summary>
    /// <param name="options">The browser storage options.</param>
    public InMemoryLocalStorageService(BrowserStorageOptions options) : this(Options.Create(options)) { }
}
