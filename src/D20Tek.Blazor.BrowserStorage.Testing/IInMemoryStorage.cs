namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// Test-time surface for the in-memory browser storage services. Provides seeding, snapshot
/// inspection, and failure-injection hooks that are not part of the production
/// <see cref="IBrowserStorageService"/> contract.
/// </summary>
public interface IInMemoryStorage
{
    /// <summary>
    /// Gets a read-only snapshot of the underlying store. Keys are returned in their
    /// prefixed form (i.e. with <see cref="BrowserStorageOptions.KeyPrefix"/> applied) and
    /// values are the serialized JSON representations written by the storage service.
    /// </summary>
    IReadOnlyDictionary<string, string> Snapshot { get; }

    /// <summary>
    /// Writes a raw serialized value to the store without raising the
    /// <see cref="IBrowserStorageService.Changed"/> event. The configured
    /// <see cref="BrowserStorageOptions.KeyPrefix"/> is applied.
    /// </summary>
    /// <param name="key">The unprefixed storage key.</param>
    /// <param name="rawJson">The raw serialized value to store.</param>
    void Seed(string key, string rawJson);

    /// <summary>
    /// Serializes <paramref name="value"/> using the same serializer as the real storage
    /// service and writes it to the store without raising the
    /// <see cref="IBrowserStorageService.Changed"/> event. The configured
    /// <see cref="BrowserStorageOptions.KeyPrefix"/> is applied.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="key">The unprefixed storage key.</param>
    /// <param name="value">The value to serialize and store.</param>
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    void Seed<T>(string key, T value);

    /// <summary>
    /// Silently seeds many raw entries at once. See <see cref="Seed(string, string)"/>.
    /// </summary>
    /// <param name="entries">The unprefixed key / raw-JSON pairs to seed.</param>
    void Seed(IEnumerable<KeyValuePair<string, string>> entries);

    /// <summary>
    /// Silently seeds many typed entries at once. See <see cref="Seed{T}(string, T)"/>.
    /// </summary>
    /// <typeparam name="T">The type of the values to serialize.</typeparam>
    /// <param name="entries">The unprefixed key / value pairs to seed.</param>
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    void Seed<T>(IEnumerable<KeyValuePair<string, T>> entries);

    /// <summary>
    /// Synchronously removes every key from the store without raising
    /// <see cref="IBrowserStorageService.Changed"/>. Intended for test setup / teardown
    /// (mirrors the silent-arrange semantics of <see cref="Seed(string, string)"/>).
    /// </summary>
    void Clear();

    /// <summary>
    /// Marks the store as unavailable.
    /// write operations return failure <see cref="StorageResult"/>s, matching the behavior of
    /// the real service when the browser blocks storage.
    /// </summary>
    void SimulateUnavailable();

    /// <summary>
    /// Restores the store to the available state after a prior
    /// <see cref="SimulateUnavailable"/> call. Existing seeded data is preserved.
    /// </summary>
    void RestoreAvailable();

    /// <summary>
    /// Raises the <see cref="IBrowserStorageService.Changed"/> event as if a cross-tab
    /// storage event had been received. Does not mutate the underlying store.
    /// </summary>
    /// <param name="key">The unprefixed key that changed.</param>
    /// <param name="oldValue">The previous raw serialized value, or <c>null</c> if the key was new.</param>
    /// <param name="newValue">The new raw serialized value, or <c>null</c> if the key was removed.</param>
    void RaiseExternalChange(string key, string? oldValue, string? newValue);
}
