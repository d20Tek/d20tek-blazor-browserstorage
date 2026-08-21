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
    /// <returns>A result indicating whether the write succeeded, with an error message on failure.</returns>
    ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken ct = default);

    /// <summary>
    /// Removes a key from storage.
    /// </summary>
    /// <param name="key">The storage key to remove.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A result indicating whether the remove succeeded, with an error message on failure.</returns>
    ValueTask<StorageResult> RemoveAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Clears all keys from storage.
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>A result indicating whether the clear succeeded, with an error message on failure.</returns>
    ValueTask<StorageResult> ClearAsync(CancellationToken ct = default);

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
    /// Checks whether the underlying browser storage is available. Storage may be unavailable
    /// when the user has blocked site data, when the browser is in a restricted private mode,
    /// or when the storage quota has been exceeded. When storage is unavailable, read operations
    /// return empty results and write operations are silently ignored (no exception is thrown).
    /// </summary>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>True if browser storage is available; otherwise false.</returns>
    ValueTask<bool> IsAvailableAsync(CancellationToken ct = default);

    /// <summary>
    /// Raised when a storage value changes.
    /// </summary>
    event EventHandler<StorageChangedEventArgs>? Changed;
}
