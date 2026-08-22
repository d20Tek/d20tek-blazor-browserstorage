using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// Framework-agnostic dependency injection extensions that register the in-memory
/// implementations of <see cref="ILocalStorageService"/> and <see cref="ISessionStorageService"/>
/// on any <see cref="IServiceCollection"/>. Usable from any test framework (xUnit, MSTest,
/// NUnit, bUnit, integration tests, etc.).
/// </summary>
public static class InMemoryBrowserStorageServiceCollectionExtensions
{
    internal const string LocalStorageKey = "D20Tek.Blazor.BrowserStorage.Testing.Local";
    internal const string SessionStorageKey = "D20Tek.Blazor.BrowserStorage.Testing.Session";

    /// <summary>
    /// Registers in-memory implementations of both <see cref="ILocalStorageService"/> and
    /// <see cref="ISessionStorageService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemoryBrowserStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null) =>
        services.AddInMemoryLocalStorage(configure).AddInMemorySessionStorage(configure);

    /// <summary>
    /// Registers an in-memory <see cref="ILocalStorageService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemoryLocalStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure(configure ?? (_ => { }));
        services.TryAddSingleton<ILocalStorageService>(sp =>
            new InMemoryLocalStorageService(sp.GetRequiredService<IOptions<BrowserStorageOptions>>()));
        services.TryAddKeyedSingleton<IInMemoryStorage>(
            LocalStorageKey,
            (sp, _) => (IInMemoryStorage)sp.GetRequiredService<ILocalStorageService>());

        return services;
    }

    /// <summary>
    /// Registers an in-memory <see cref="ISessionStorageService"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemorySessionStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.Configure(configure ?? (_ => { }));
        services.TryAddSingleton<ISessionStorageService>(sp =>
            new InMemorySessionStorageService(sp.GetRequiredService<IOptions<BrowserStorageOptions>>()));
        services.TryAddKeyedSingleton<IInMemoryStorage>(
            SessionStorageKey,
            (sp, _) => (IInMemoryStorage)sp.GetRequiredService<ISessionStorageService>());

        return services;
    }

    /// <summary>
    /// Resolves the in-memory local storage handle for arrange / assert operations.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public static IInMemoryStorage GetInMemoryLocalStorage(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredKeyedService<IInMemoryStorage>(LocalStorageKey);
    }

    /// <summary>
    /// Resolves the in-memory session storage handle for arrange / assert operations.
    /// </summary>
    /// <param name="services">The service provider.</param>
    public static IInMemoryStorage GetInMemorySessionStorage(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.GetRequiredKeyedService<IInMemoryStorage>(SessionStorageKey);
    }

    /// <summary>
    /// Removes any prior <see cref="ILocalStorageService"/> and <see cref="ISessionStorageService"/>
    /// registrations (including the production ones added by
    /// <c>ServiceCollectionExtensions.AddBrowserStorage</c>) and registers the in-memory
    /// implementations in their place. Intended for integration-test setups such as
    /// <c>WebApplicationFactory</c> where the real registrations must be swapped out.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ReplaceWithInMemoryBrowserStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null) =>
        services.ReplaceWithInMemoryLocalStorage(configure)
                .ReplaceWithInMemorySessionStorage(configure);

    /// <summary>
    /// Removes any prior <see cref="ILocalStorageService"/> registration and replaces it with
    /// the in-memory implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ReplaceWithInMemoryLocalStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RemoveAll<ILocalStorageService>();
        return services.AddInMemoryLocalStorage(configure);
    }

    /// <summary>
    /// Removes any prior <see cref="ISessionStorageService"/> registration and replaces it with
    /// the in-memory implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for <see cref="BrowserStorageOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ReplaceWithInMemorySessionStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.RemoveAll<ISessionStorageService>();
        return services.AddInMemorySessionStorage(configure);
    }
}
