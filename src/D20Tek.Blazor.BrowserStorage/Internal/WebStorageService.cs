using System.Diagnostics.CodeAnalysis;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal abstract class WebStorageService : IBrowserStorageService
{
    private const string ModulePath = "./_content/D20Tek.Blazor.BrowserStorage/BrowserStorageInterop.js";

    private readonly string _storageName;
    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    private DotNetObjectReference<WebStorageService>? _dotNetRef;
    private IJSObjectReference? _module;
    private int _listenerId = -1;

    protected WebStorageService(string storageName, IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    {
        _storageName = storageName;
        _jsRuntime = jsRuntime;
        _options = options.Value;
        _jsonOptions = _options.JsonOptions ?? new(JsonSerializerDefaults.Web);
    }

    [ExcludeFromCodeCoverage]
    public event EventHandler<StorageChangedEventArgs>? Changed
    {
        add
        {
            ChangedInternal += value;
            if (ChangedInternal is not null && _module is null)
            {
                InitializeListenerAsync();
            }
        }
        remove => ChangedInternal -= value;
    }

    private event EventHandler<StorageChangedEventArgs>? ChangedInternal;

    private async void InitializeListenerAsync() => await EnsureListenerAsync();

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

    public async ValueTask SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct = default)
    {
        foreach (var (key, value) in items)
        {
            var prefixedKey = _options.PrefixKey(key);
            var json = StorageSerializer.Serialize(value, _jsonOptions);
            await JsInterop.SetItemAsync(_jsRuntime, _storageName, prefixedKey, json, ct);
        }
    }

    public async ValueTask RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        foreach (var key in keys)
        {
            await JsInterop.RemoveItemAsync(_jsRuntime, _storageName, _options.PrefixKey(key), ct);
        }
    }

    private object? DeserializeRaw(string? json) => json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        ChangedInternal?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));

    private async ValueTask EnsureListenerAsync()
    {
        if (_module is null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            _dotNetRef = DotNetObjectReference.Create(this);
            _listenerId = await _module.InvokeAsync<int>("addStorageListener", _dotNetRef, _storageName);
        }
    }

    [JSInvokable]
    public void OnStorageChanged(string? key, string? oldValue, string? newValue)
    {
        if (key is null) return;

        RaiseChanged(_options.StripPrefix(key), oldValue, newValue);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null && _listenerId >= 0)
        {
            await _module.InvokeVoidAsync("removeStorageListener", _listenerId);
            await _module.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
