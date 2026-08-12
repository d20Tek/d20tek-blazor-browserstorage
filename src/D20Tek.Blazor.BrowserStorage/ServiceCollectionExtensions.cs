using D20Tek.Blazor.BrowserStorage.Internal;
using Microsoft.Extensions.DependencyInjection;

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
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBrowserStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null) =>
        services.AddLocalStorage(configure)
                .AddSessionStorage(configure);

    /// <summary>
    /// Registers the local storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null) =>
        services.Configure(configure ?? (_ => { }))
                .AddScoped<ILocalStorageService, LocalStorageService>();

    /// <summary>
    /// Registers the session storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null) =>
        services.Configure(configure ?? (_ => { }))
                .AddScoped<ISessionStorageService, SessionStorageService>();
}
