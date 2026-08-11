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
        Action<BrowserStorageOptions>? configure = null)
    {
        AddLocalStorage(services, configure);
        AddSessionStorage(services, configure);
        return services;
    }

    /// <summary>
    /// Registers the local storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        // TODO: Register LocalStorageService implementation
        throw new NotImplementedException();
    }

    /// <summary>
    /// Registers the session storage service.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for storage options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionStorage(
        this IServiceCollection services,
        Action<BrowserStorageOptions>? configure = null)
    {
        // TODO: Register SessionStorageService implementation
        throw new NotImplementedException();
    }
}
