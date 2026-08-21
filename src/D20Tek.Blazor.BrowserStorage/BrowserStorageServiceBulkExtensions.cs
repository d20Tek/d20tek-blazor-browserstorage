namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Extension methods for bulk storage operations on <see cref="IBrowserStorageService"/>.
/// </summary>
public static class BrowserStorageServiceBulkExtensions
{
    /// <summary>
    /// Sets multiple key-value pairs in storage.
    /// </summary>
    /// <param name="service">The storage service.</param>
    /// <param name="items">The key-value pairs to store.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public static async ValueTask SetMultipleAsync(
        this IBrowserStorageService service,
        IEnumerable<KeyValuePair<string, object>> items,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var (key, value) in items)
        {
            await service.SetAsync(key, value, ct);
        }
    }

    /// <summary>
    /// Removes multiple keys from storage.
    /// </summary>
    /// <param name="service">The storage service.</param>
    /// <param name="keys">The keys to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    public static async ValueTask RemoveMultipleAsync(
        this IBrowserStorageService service,
        IEnumerable<string> keys,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            await service.RemoveAsync(key, ct);
        }
    }
}
