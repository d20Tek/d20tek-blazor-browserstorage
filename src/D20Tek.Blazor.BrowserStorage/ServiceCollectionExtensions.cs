using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Extension methods for registering browser storage services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers both local and session storage services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <param name="lifetime">The service lifetime (defaults to Scoped).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBrowserStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped) =>
        services.AddLocalStorage(configure, lifetime)
                .AddSessionStorage(configure, lifetime);

    /// <summary>
    /// Registers the local storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <param name="lifetime">The service lifetime (defaults to Scoped).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Configure(configure ?? (_ => { }));
        services.TryAdd(new ServiceDescriptor(typeof(ILocalStorageService), typeof(LocalStorageService), lifetime));
        return services;
    }

    /// <summary>
    /// Registers the session storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <param name="lifetime">The service lifetime (defaults to Scoped).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        services.Configure(configure ?? (_ => { }));
        services.TryAdd(new ServiceDescriptor(typeof(ISessionStorageService), typeof(SessionStorageService), lifetime));
        return services;
    }
}
