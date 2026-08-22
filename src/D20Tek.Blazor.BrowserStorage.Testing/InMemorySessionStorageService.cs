namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// In-memory implementation of <see cref="ISessionStorageService"/> for tests. Also implements
/// <see cref="IInMemoryStorage"/> for seeding and inspection. Can be used directly from any
/// test framework (bUnit, MSTest, xUnit, NUnit) without additional adapters.
/// </summary>
/// <param name="options">The browser storage options.</param>
public sealed class InMemorySessionStorageService(IOptions<BrowserStorageOptions> options)
    : InMemoryWebStorageService("sessionStorage", options), ISessionStorageService
{
    /// <summary>Initializes a new instance with default <see cref="BrowserStorageOptions"/>.</summary>
    public InMemorySessionStorageService() : this(new BrowserStorageOptions()) { }

    /// <summary>Initializes a new instance with the specified <see cref="BrowserStorageOptions"/>.</summary>
    /// <param name="options">The browser storage options.</param>
    public InMemorySessionStorageService(BrowserStorageOptions options) : this(Options.Create(options)) { }
}
