using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal abstract class WebStorageService : IBrowserStorageService
{
    private static readonly IReadOnlyList<string> EmptyKeys = Array.Empty<string>();

    private readonly string _storageName;
    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly StorageListenerManager _listenerManager;
    private bool? _isAvailable;

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

    public async ValueTask<bool> IsAvailableAsync(CancellationToken ct = default)
    {
        if (_isAvailable is bool cached) return cached;

        try
        {
            _isAvailable = await JsInterop.IsStorageAvailableAsync(_jsRuntime, _storageName, ct);
        }
        catch
        {
            _isAvailable = false;
        }

        return _isAvailable.Value;
    }

    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return new StorageResult<T>(false, default);

        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        return (json is null)
            ? new StorageResult<T>(false, default)
            : new StorageResult<T>(true, StorageSerializer.Deserialize<T>(json, _jsonOptions));
    }

    public async ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return;

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        var json = StorageSerializer.Serialize(value, _jsonOptions);

        await JsInterop.SetItemAsync(_jsRuntime, _storageName, prefixedKey, json, ct);

        RaiseChanged(key, DeserializeRaw(oldJson), value);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return;

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);

        await JsInterop.RemoveItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        RaiseChanged(key, DeserializeRaw(oldJson), null);
    }

    public async ValueTask ClearAsync(CancellationToken ct = default)
    {
        if (!await IsAvailableAsync(ct)) return;

        await JsInterop.ClearAsync(_jsRuntime, _storageName, ct);
    }

    public async ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default)
    {
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
