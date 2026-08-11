using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class LocalStorageService : ILocalStorageService
{
    private const string StorageName = "localStorage";

    private readonly IJSRuntime _jsRuntime;
    private readonly BrowserStorageOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public LocalStorageService(IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    {
        _jsRuntime = jsRuntime;
        _options = options.Value;
        _jsonOptions = _options.JsonOptions ?? new JsonSerializerOptions(JsonSerializerDefaults.Web);
    }

    public event EventHandler<StorageChangedEventArgs>? Changed;

    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default)
    {
        var json = await JsInterop.GetItemAsync(_jsRuntime, StorageName, PrefixKey(key), ct);
        return (json is null)
            ? new StorageResult<T>(false, default)
            : new StorageResult<T>(true, StorageSerializer.Deserialize<T>(json, _jsonOptions));
    }

    public async ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default)
    {
        var prefixedKey = PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, StorageName, prefixedKey, ct);
        var json = StorageSerializer.Serialize(value, _jsonOptions);

        await JsInterop.SetItemAsync(_jsRuntime, StorageName, prefixedKey, json, ct);

        RaiseChanged(key, DeserializeRaw(oldJson), value);
    }

    public async ValueTask RemoveAsync(string key, CancellationToken ct = default)
    {
        var prefixedKey = PrefixKey(key);
        var oldJson = await JsInterop.GetItemAsync(_jsRuntime, StorageName, prefixedKey, ct);

        await JsInterop.RemoveItemAsync(_jsRuntime, StorageName, prefixedKey, ct);

        RaiseChanged(key, DeserializeRaw(oldJson), null);
    }

    public ValueTask ClearAsync(CancellationToken ct = default) => JsInterop.ClearAsync(_jsRuntime, StorageName, ct);

    public async ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default)
    {
        var json = await JsInterop.GetItemAsync(_jsRuntime, StorageName, PrefixKey(key), ct);
        return json is not null;
    }

    public ValueTask<int> LengthAsync(CancellationToken ct = default) =>
        JsInterop.LengthAsync(_jsRuntime, StorageName, ct);

    public async ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken ct = default)
    {
        var length = await JsInterop.LengthAsync(_jsRuntime, StorageName, ct);
        var keys = new List<string>(length);

        for (var i = 0; i < length; i++)
        {
            var key = await JsInterop.KeyAsync(_jsRuntime, StorageName, i, ct);
            if (key is not null)
            {
                keys.Add(StripPrefix(key));
            }
        }

        return keys;
    }

    public async ValueTask SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct = default)
    {
        foreach (var (key, value) in items)
        {
            var prefixedKey = PrefixKey(key);
            var json = StorageSerializer.Serialize(value, _jsonOptions);
            await JsInterop.SetItemAsync(_jsRuntime, StorageName, prefixedKey, json, ct);
        }
    }

    public async ValueTask RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct = default)
    {
        foreach (var key in keys)
        {
            await JsInterop.RemoveItemAsync(_jsRuntime, StorageName, PrefixKey(key), ct);
        }
    }

    private string PrefixKey(string key) => $"{_options.KeyPrefix}{key}";

    private string StripPrefix(string key) =>
        _options.KeyPrefix.Length > 0 && key.StartsWith(_options.KeyPrefix, StringComparison.Ordinal)
            ? key[_options.KeyPrefix.Length..]
            : key;

    private object? DeserializeRaw(string? json) =>
        json is null ? null : StorageSerializer.Deserialize<object>(json, _jsonOptions);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        Changed?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));
}
