namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Extension methods for bulk storage operations on <see cref="IBrowserStorageService"/>.
/// </summary>
public static class BrowserStorageServiceBulkExtensions
{
    /// <summary>
    /// Sets multiple key-value pairs in storage. Stops on the first failure.
    /// </summary>
    /// <param name="service">The storage service.</param>
    /// <param name="items">The key-value pairs to store.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A success result if all writes succeeded; otherwise the first failing result.</returns>
    public static async ValueTask<StorageResult> SetMultipleAsync(
        this IBrowserStorageService service,
        IEnumerable<KeyValuePair<string, object>> items,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(items);

        foreach (var (key, value) in items)
        {
            var result = await service.SetAsync(key, value, ct);
            if (!result.IsSuccess) return result;
        }

        return StorageResult.Success();
    }

    /// <summary>
    /// Removes multiple keys from storage. Stops on the first failure.
    /// </summary>
    /// <param name="service">The storage service.</param>
    /// <param name="keys">The keys to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A success result if all removes succeeded; otherwise the first failing result.</returns>
    public static async ValueTask<StorageResult> RemoveMultipleAsync(
        this IBrowserStorageService service,
        IEnumerable<string> keys,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(service);
        ArgumentNullException.ThrowIfNull(keys);

        foreach (var key in keys)
        {
            var result = await service.RemoveAsync(key, ct);
            if (!result.IsSuccess) return result;
        }

        return StorageResult.Success();
    }
}
