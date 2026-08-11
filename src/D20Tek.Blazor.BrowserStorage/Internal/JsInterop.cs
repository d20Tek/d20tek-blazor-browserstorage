using Microsoft.JSInterop;

namespace D20Tek.Blazor.BrowserStorage.Internal;

/// <summary>
/// Internal helper for browser storage JS interop calls.
/// </summary>
internal static class JsInterop
{
    public static ValueTask<string?> GetItemAsync(
        IJSRuntime js, string storageName, string key, CancellationToken ct) =>
        js.InvokeAsync<string?>($"{storageName}.getItem", ct, key);

    public static ValueTask SetItemAsync(
        IJSRuntime js, string storageName, string key, string json, CancellationToken ct) =>
        js.InvokeVoidAsync($"{storageName}.setItem", ct, key, json);

    public static ValueTask RemoveItemAsync(
        IJSRuntime js, string storageName, string key, CancellationToken ct) =>
        js.InvokeVoidAsync($"{storageName}.removeItem", ct, key);

    public static ValueTask ClearAsync(
        IJSRuntime js, string storageName, CancellationToken ct) =>
        js.InvokeVoidAsync($"{storageName}.clear", ct);

    public static ValueTask<int> LengthAsync(
        IJSRuntime js, string storageName, CancellationToken ct) =>
        js.InvokeAsync<int>($"eval", ct, $"{storageName}.length");

    public static ValueTask<string?> KeyAsync(
        IJSRuntime js, string storageName, int index, CancellationToken ct) =>
        js.InvokeAsync<string?>($"{storageName}.key", ct, index);
}
