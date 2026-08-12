using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal abstract class WebStorageService : IBrowserStorageService
{
    private readonly string _storageName;
    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly StorageListenerManager _listenerManager;

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

    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        return (json is null)
            ? new StorageResult<T>(false, default)
            : new StorageResult<T>(true, StorageSerializer.Deserialize<T>(json, _jsonOptions));
    }

    public async ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        var json = StorageSerializer.Serialize(value, _jsonOptions);

        await JsInterop.SetItemAsync(_jsRuntime, _storageName, prefixedKey, json, ct);

        RaiseChanged(key, DeserializeRaw(oldJson), value);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, _storageName, prefixedKey, ct);

        await JsInterop.RemoveItemAsync(_jsRuntime, _storageName, prefixedKey, ct);
        RaiseChanged(key, DeserializeRaw(oldJson), null);
    }

    public ValueTask ClearAsync(CancellationToken ct = default) => JsInterop.ClearAsync(_jsRuntime, _storageName, ct);

    public async ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default)
    {
        var json = await JsInterop.GetItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        return json is not null;
    }

    public ValueTask<int> LengthAsync(CancellationToken ct = default) => JsInterop.LengthAsync(_jsRuntime, _storageName, ct);

    public async ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken ct = default)
    {
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

    private object? DeserializeRaw(string? json) => json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        ChangedInternal?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));

    public ValueTask DisposeAsync() => _listenerManager.DisposeAsync();
}

