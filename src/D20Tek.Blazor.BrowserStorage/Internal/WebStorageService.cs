namespace D20Tek.Blazor.BrowserStorage.Internal;

internal abstract class WebStorageService : IBrowserStorageService
{
    private static readonly IReadOnlyList<string> EmptyKeys = [];

    private readonly string _storageName;
    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly StorageListenerManager _listenerManager;
    private readonly StorageAvailabilityGate _availability;

    protected WebStorageService(string storageName, IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    {
        _storageName = storageName;
        _jsRuntime = jsRuntime;
        _options = options.Value;
        _jsonOptions = _options.JsonOptions ?? new(JsonSerializerDefaults.Web);

        // Freeze the serializer options so any post-registration mutation cannot silently
        // change (de)serialization behavior at runtime. Passing populateMissingResolver: true
        // ensures a reflection-based resolver is attached when none was configured, matching
        // the default behavior of JsonSerializer.Serialize/Deserialize.
        if (!_jsonOptions.IsReadOnly)
        {
            _jsonOptions.MakeReadOnly(populateMissingResolver: true);
        }

        _listenerManager = new StorageListenerManager(jsRuntime, storageName, _options, RaiseChanged);
        _availability = new StorageAvailabilityGate(storageName, jsRuntime);
    }

    internal StorageListenerManager ListenerManager => _listenerManager;

    private int _listenerInitStarted;

    [ExcludeFromCodeCoverage]
    public event EventHandler<StorageChangedEventArgs>? Changed
    {
        add
        {
            ChangedInternal += value;

            // Only one subscriber may kick off the JS listener import; concurrent add
            // accessors would otherwise race on IsInitialized and double-import the module.
            if (value is not null && Interlocked.CompareExchange(ref _listenerInitStarted, 1, 0) == 0)
            {
                _listenerManager.InitializeListenerAsync();
            }
        }
        remove => ChangedInternal -= value;
    }

    private event EventHandler<StorageChangedEventArgs>? ChangedInternal;

    public ValueTask<bool> IsAvailableAsync(CancellationToken ct = default) => _availability.IsAvailableAsync(ct);

    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct)) return StorageResult<T>.Failure(StorageMessages.Unavailable(_storageName));

        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        if (json is null) return StorageResult<T>.Failure(StorageMessages.KeyNotFound(_storageName, key));

        try
        {
            return StorageResult<T>.Success(StorageSerializer.Deserialize<T>(json, _jsonOptions));
        }
        catch (Exception ex) when (ex is JsonException
                                      or FormatException
                                      or OverflowException
                                      or NotSupportedException
                                      or ArgumentException)
        {
            // Stored value is corrupt, was written under a different schema/type, or the target type cannot be deserialized.
            // Honor the no-exception contract of GetAsync and return failure instead of surfacing raw parser errors to callers.
            return StorageResult<T>.Failure(ex.Message);
        }
    }

    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public async ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct)) return StorageResult.Failure(StorageMessages.Unavailable(_storageName));

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        var json = StorageSerializer.Serialize(value, _jsonOptions);

        try
        {
            await JsInterop.SetItemAsync(_jsRuntime, _storageName, prefixedKey, json, ct);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            // The browser storage may be full, disabled, or the circuit may have disconnected.
            return StorageResult.Failure(StorageMessages.WriteFailed(_storageName, key, ex));
        }

        RaiseChanged(key, DeserializeRaw(oldJson), value);
        return StorageResult.Success();
    }

    public async ValueTask<StorageResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct)) return StorageResult.Failure(StorageMessages.Unavailable(_storageName));

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);

        try
        {
            await JsInterop.RemoveItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            return StorageResult.Failure(StorageMessages.RemoveFailed(_storageName, key, ex));
        }

        RaiseChanged(key, DeserializeRaw(oldJson), null);
        return StorageResult.Success();
    }

    // Clears the entire browser storage area (localStorage or sessionStorage) for the current
    // origin. This is destructive and area-wide: it removes every key, including those written
    // by other libraries or app code sharing the same storage. The configured KeyPrefix is NOT
    // honored here — enumerate GetKeysAsync + RemoveAsync to scope the delete to this service.
    public async ValueTask<StorageResult> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken)) return StorageResult.Failure(StorageMessages.Unavailable(_storageName));

        try
        {
            await JsInterop.ClearAsync(_jsRuntime, _storageName, cancellationToken);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            return StorageResult.Failure(StorageMessages.ClearFailed(_storageName, ex));
        }

        return StorageResult.Success();
    }

    public async ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct)) return false;

        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        return json is not null;
    }

    public async ValueTask<int> LengthAsync(CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return 0;

        return await JsInterop.LengthAsync(_jsRuntime, _storageName, ct);
    }

    public async ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return EmptyKeys;

        var length = await JsInterop.LengthAsync(_jsRuntime, _storageName, ct);
        var keys = new List<string>(length);

        for (var i = 0; i < length; i++)
        {
            var key = await JsInterop.KeyAsync(_jsRuntime, _storageName, i, ct);
            if (key is not null)
            {
                keys.Add(_options.StripPrefix(key));
            }
        }

        return keys;
    }

    [UnconditionalSuppressMessage("Trimming", "IL2026",
        Justification = "DeserializeRaw is used to surface previous stored values on Changed events. Callers who rely on " +
                        "the Changed event with non-primitive types are already surfaced through GetAsync<T>/SetAsync<T> warnings.")]
    [UnconditionalSuppressMessage("AOT", "IL3050",
        Justification = "See IL2026 justification above.")]
    private object? DeserializeRaw(string? json) =>
        json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        ChangedInternal?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));

    public ValueTask DisposeAsync() => _listenerManager.DisposeAsync();
}
