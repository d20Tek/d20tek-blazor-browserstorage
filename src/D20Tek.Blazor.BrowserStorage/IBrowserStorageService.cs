namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Provides typed, async access to browser storage (localStorage or sessionStorage).
/// </summary>
public interface IBrowserStorageService : IAsyncDisposable
{
    /// <summary>
    /// Gets a value from storage by key.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the value as.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A result indicating success and the deserialized value.</returns>
    ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default);

    /// <summary>
    /// Sets a value in storage.
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The storage key.</param>
    /// <param name="value">The value to serialize and store.</param>
    /// <param name="ct">Optional cancellation token.</param>
    ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default);

    /// <summary>
    /// Removes a key from storage.
    /// </summary>
    /// <param name="key">The storage key to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    ValueTask RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Clears all keys from storage.
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    ValueTask ClearAsync(CancellationToken ct = default);

    /// <summary>
    /// Checks whether a key exists in storage.
    /// </summary>
    /// <param name="key">The storage key to check.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>True if the key exists; otherwise false.</returns>
    ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Gets the number of keys in storage.
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>The number of stored keys.</returns>
    ValueTask<int> LengthAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets all keys in storage.
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A read-only list of storage keys.</returns>
    ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken ct = default);

    /// <summary>
    /// Sets multiple key-value pairs in storage.
    /// </summary>
    /// <param name="items">The key-value pairs to store.</param>
    /// <param name="ct">Optional cancellation token.</param>
    ValueTask SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct = default);

    /// <summary>
    /// Removes multiple keys from storage.
    /// </summary>
    /// <param name="keys">The keys to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    ValueTask RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct = default);

    /// <summary>
    /// Raised when a storage value changes.
    /// </summary>
    event EventHandler<StorageChangedEventArgs>? Changed;
}
