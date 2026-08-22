namespace D20Tek.Blazor.BrowserStorage.Testing;

/// <summary>
/// Base class for the in-memory implementations of <see cref="IBrowserStorageService"/>.
/// Mirrors the behavior of the real <see cref="WebStorageService"/>: honors
/// <see cref="BrowserStorageOptions.KeyPrefix"/> and <see cref="BrowserStorageOptions.JsonOptions"/>,
/// serializes values through the same <see cref="StorageSerializer"/>, and raises the
/// <see cref="Changed"/> event with <see cref="StorageChangedEventArgs"/> using the same semantics.
/// </summary>
public abstract class InMemoryWebStorageService : IBrowserStorageService, IInMemoryStorage, IDisposable
{
    private static readonly IReadOnlyList<string> EmptyKeys = [];

    private readonly string _storageName;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ConcurrentDictionary<string, string> _store = new(StringComparer.Ordinal);
    private volatile bool _available = true;

    /// <summary>Initializes the in-memory store.</summary>
    /// <param name="storageName">The storage area name ("localStorage" or "sessionStorage") used in error messages.</param>
    /// <param name="options">The browser storage options.</param>
    protected InMemoryWebStorageService(string storageName, IOptions<BrowserStorageOptions> options)
    {
        _storageName = storageName;
        _options = options.Value;
        _jsonOptions = _options.JsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);

        if (!_jsonOptions.IsReadOnly)
        {
            _jsonOptions.MakeReadOnly(populateMissingResolver: true);
        }
    }

    /// <inheritdoc />
    public event EventHandler<StorageChangedEventArgs>? Changed;

    /// <inheritdoc />
    public ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
        new(_available);

    /// <inheritdoc />
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!_available) return new(StorageResult<T>.Failure(StorageMessages.Unavailable(_storageName)));

        if (!_store.TryGetValue(_options.PrefixKey(key), out var json))
        {
            return new(StorageResult<T>.Failure(StorageMessages.KeyNotFound(_storageName, key)));
        }

        try
        {
            return new(StorageResult<T>.Success(StorageSerializer.Deserialize<T>(json, _jsonOptions)));
        }
        catch (Exception ex) when (ex is JsonException
                                      or FormatException
                                      or OverflowException
                                      or NotSupportedException
                                      or ArgumentException)
        {
            return new(StorageResult<T>.Failure(ex.Message));
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!_available) return new(StorageResult.Failure(StorageMessages.Unavailable(_storageName)));

        var prefixedKey = _options.PrefixKey(key);
        _store.TryGetValue(prefixedKey, out var oldJson);
        var json = StorageSerializer.Serialize(value, _jsonOptions);
        _store[prefixedKey] = json;

        RaiseChanged(key, DeserializeRaw(oldJson), value);
        return new(StorageResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<StorageResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!_available) return new(StorageResult.Failure(StorageMessages.Unavailable(_storageName)));

        var prefixedKey = _options.PrefixKey(key);
        _store.TryRemove(prefixedKey, out var oldJson);

        RaiseChanged(key, DeserializeRaw(oldJson), null);
        return new(StorageResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<StorageResult> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_available) return new(StorageResult.Failure(StorageMessages.Unavailable(_storageName)));

        _store.Clear();
        return new(StorageResult.Success());
    }

    /// <inheritdoc />
    public ValueTask<bool> ContainsKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!_available) return new(false);

        return new(_store.ContainsKey(_options.PrefixKey(key)));
    }

    /// <inheritdoc />
    public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default) =>
        new(_available ? _store.Count : 0);

    /// <inheritdoc />
    public ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        if (!_available) return new(EmptyKeys);

        var keys = new List<string>(_store.Count);
        foreach (var key in _store.Keys)
        {
            keys.Add(_options.StripPrefix(key));
        }

        return new(keys);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Synchronous disposal. The in-memory store holds no unmanaged resources; this exists so
    /// Microsoft.Extensions.DependencyInjection can tear down its container without requiring
    /// async disposal (bUnit's <c>BunitContext.Dispose()</c> takes the sync path).
    /// </summary>
    public void Dispose() { }

    // --- IInMemoryStorage ------------------------------------------------------------------

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> Snapshot =>
        new Dictionary<string, string>(_store, StringComparer.Ordinal);

    /// <inheritdoc />
    public void Seed(string key, string rawJson)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(rawJson);
        _store[_options.PrefixKey(key)] = rawJson;
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public void Seed<T>(string key, T value)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        _store[_options.PrefixKey(key)] = StorageSerializer.Serialize(value, _jsonOptions);
    }

    /// <inheritdoc />
    public void SimulateUnavailable() => _available = false;

    /// <inheritdoc />
    public void RestoreAvailable() => _available = true;

    /// <inheritdoc />
    public void RaiseExternalChange(string key, string? oldValue, string? newValue)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        RaiseChanged(key, DeserializeRaw(oldValue), DeserializeRaw(newValue));
    }

    // --- helpers ---------------------------------------------------------------------------

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "DeserializeRaw is used to surface previous stored values on Changed events. Callers who rely on " +
                        "the Changed event with non-primitive types are already surfaced through GetAsync<T>/SetAsync<T> warnings.")]
    [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "See IL2026 justification above.")]
    private object? DeserializeRaw(string? json) =>
        json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        Changed?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));
}
