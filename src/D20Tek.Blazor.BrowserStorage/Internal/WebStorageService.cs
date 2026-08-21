using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal abstract class WebStorageService : IBrowserStorageService
{
    private static readonly IReadOnlyList<string> EmptyKeys = [];

    private readonly string _storageName;
    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly StorageListenerManager _listenerManager;
    private Lazy<Task<bool>>? _availabilityCheck;

    protected WebStorageService(string storageName, IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    {
        _storageName = storageName;
        _jsRuntime = jsRuntime;
        _options = options.Value;
        _jsonOptions = _options.JsonOptions ?? new(JsonSerializerDefaults.Web);
        _listenerManager = new StorageListenerManager(jsRuntime, storageName, _options, RaiseChanged);
    }

    internal StorageListenerManager ListenerManager => _listenerManager;

    [ExcludeFromCodeCoverage]
    public event EventHandler<StorageChangedEventArgs>? Changed
    {
        add
        {
            ChangedInternal += value;
            if (ChangedInternal is not null && !_listenerManager.IsInitialized)
            {
                _listenerManager.InitializeListenerAsync();
            }
        }
        remove => ChangedInternal -= value;
    }

    private event EventHandler<StorageChangedEventArgs>? ChangedInternal;

    public ValueTask<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        var lazy = LazyInitializer.EnsureInitialized(
            ref _availabilityCheck,
            () => new Lazy<Task<bool>>(
                () => ProbeAvailabilityAsync(),
                LazyThreadSafetyMode.ExecutionAndPublication));
        return new ValueTask<bool>(lazy.Value.WaitAsync(ct));
    }

    private async Task<bool> ProbeAvailabilityAsync()
    {
        try
        {
            return await JsInterop.IsStorageAvailableAsync(_jsRuntime, _storageName, CancellationToken.None)
                                  .ConfigureAwait(false);
        }
        catch
        {
            return false;
        }
    }

    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct))
            return StorageResult<T>.Failure($"Browser {_storageName} is not available.");

        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        if (json is null) return StorageResult<T>.Failure($"Key '{key}' not found in {_storageName}.");

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

    public async ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct))
            return StorageResult.Failure($"Browser {_storageName} is not available.");

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        var json = StorageSerializer.Serialize(value, _jsonOptions);

        try
        {
            await JsInterop.SetItemAsync(_jsRuntime, _storageName, prefixedKey, json, ct);
        }
        catch (JSException ex)
        {
            // The browser storage may be full, disabled, or otherwise unavailable.
            return StorageResult.Failure($"Failed to write value to {_storageName} for key '{key}': {ex.Message}");
        }

        RaiseChanged(key, DeserializeRaw(oldJson), value);
        return StorageResult.Success();
    }

    public async ValueTask<StorageResult> RemoveAsync(string key, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(ct))
            return StorageResult.Failure($"Browser {_storageName} is not available.");

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);

        try
        {
            await JsInterop.RemoveItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        }
        catch (JSException ex)
        {
            return StorageResult.Failure($"Failed to remove value from {_storageName} for key '{key}': {ex.Message}");
        }

        RaiseChanged(key, DeserializeRaw(oldJson), null);
        return StorageResult.Success();
    }

    public async ValueTask<StorageResult> ClearAsync(CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct))
            return StorageResult.Failure($"Browser {_storageName} is not available.");

        try
        {
            await JsInterop.ClearAsync(_jsRuntime, _storageName, ct);
        }
        catch (JSException ex)
        {
            return StorageResult.Failure($"Failed to clear {_storageName}: {ex.Message}");
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

    private object? DeserializeRaw(string? json) => 
        json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        ChangedInternal?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));

    public ValueTask DisposeAsync() => _listenerManager.DisposeAsync();
}
