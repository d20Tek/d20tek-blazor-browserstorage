namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// bUnit <see cref="BunitContext"/> extensions that register in-memory implementations of
/// <see cref="ILocalStorageService"/>, <see cref="ISessionStorageService"/>, and
/// <see cref="ICookieStorageService"/> for component and service tests. These are thin
/// adapters over <see cref="InMemoryBrowserStorageServiceCollectionExtensions"/> and
/// <see cref="InMemoryCookieStorageServiceCollectionExtensions"/>.
/// </summary>
public static class BrowserStorageTestContextExtensions
{
    /// <summary>
    /// Registers in-memory local and session storage services on the bUnit
    /// <paramref name="context"/>.
    /// </summary>
    public static BunitContext AddBrowserStorage(this BunitContext context, Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Services.AddInMemoryBrowserStorage(configure);
        return context;
    }

    /// <summary>
    /// Registers an in-memory <see cref="ILocalStorageService"/> on the bUnit
    /// <paramref name="context"/>.
    /// </summary>
    public static BunitContext AddLocalStorage(this BunitContext context, Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Services.AddInMemoryLocalStorage(configure);
        return context;
    }

    /// <summary>
    /// Registers an in-memory <see cref="ISessionStorageService"/> on the bUnit
    /// <paramref name="context"/>.
    /// </summary>
    public static BunitContext AddSessionStorage(this BunitContext context, Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Services.AddInMemorySessionStorage(configure);
        return context;
    }

    /// <summary>
    /// Registers an in-memory <see cref="ICookieStorageService"/> on the bUnit
    /// <paramref name="context"/>.
    /// </summary>
    public static BunitContext AddCookieStorage(this BunitContext context, Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.Services.AddInMemoryCookieStorage(configure);
        return context;
    }

    /// <summary>
    /// Gets the in-memory local storage handle for arrange / assert operations.
    /// </summary>
    public static IInMemoryStorage GetLocalStorage(this BunitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Services.GetInMemoryLocalStorage();
    }

    /// <summary>
    /// Gets the in-memory session storage handle for arrange / assert operations.
    /// </summary>
    public static IInMemoryStorage GetSessionStorage(this BunitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Services.GetInMemorySessionStorage();
    }

    /// <summary>
    /// Gets the in-memory cookie storage handle for arrange / assert operations.
    /// </summary>
    public static IInMemoryStorage GetCookieStorage(this BunitContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Services.GetInMemoryCookieStorage();
    }
}
