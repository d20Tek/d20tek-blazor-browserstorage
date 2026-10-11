using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// Framework-agnostic dependency injection extensions that register the in-memory
/// implementation of <see cref="ICookieStorageService"/> on any <see cref="IServiceCollection"/>.
/// Usable from any test framework (xUnit, MSTest, NUnit, bUnit, integration tests, etc.).
/// </summary>
public static class InMemoryCookieStorageServiceCollectionExtensions
{
    internal const string CookieStorageKey = "D20Tek.Blazor.BrowserStorage.Testing.Cookie";

    /// <summary>
    /// Registers an in-memory <see cref="ICookieStorageService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemoryCookieStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure(configure ?? (_ => { }));
        services.TryAddSingleton<ICookieStorageService>(sp =>
            new InMemoryCookieStorageService(sp.GetRequiredService<IOptions<BrowserStorageOptions>>()));
        services.TryAddKeyedSingleton<IInMemoryStorage>(
            CookieStorageKey,
            (sp, _) => (IInMemoryStorage)sp.GetRequiredService<ICookieStorageService>());

        return services;
    }

    /// <summary>
    /// Resolves the in-memory cookie storage handle for arrange / assert operations.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public static IInMemoryStorage GetInMemoryCookieStorage(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredKeyedService<IInMemoryStorage>(CookieStorageKey);
    }

    /// <summary>
    /// Removes any prior <see cref="ICookieStorageService"/> registration (including the
    /// production registration added by <c>ServiceCollectionExtensions.AddCookieStorage</c>)
    /// and replaces it with the in-memory implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ReplaceWithInMemoryCookieStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RemoveAll<ICookieStorageService>();
        return services.AddInMemoryCookieStorage(configure);
    }
}
