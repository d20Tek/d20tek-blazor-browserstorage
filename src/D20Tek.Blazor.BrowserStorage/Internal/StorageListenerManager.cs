namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class StorageListenerManager(
    IJSRuntime jsRuntime,
    string storageName,
    BrowserStorageOptions options,
    Action<string, object?, object?> onChanged) : IAsyncDisposable
{
    private const string ModulePath = "./_content/D20Tek.Blazor.BrowserStorage/BrowserStorageInterop.js";
    private DotNetObjectReference<StorageListenerManager>? _dotNetRef;
    private IJSObjectReference? _module;
    private int _listenerId = -1;

    public bool IsInitialized => _module is not null;

    public async ValueTask InitializeAsync()
    {
        if (_module is null)
        {
            var module = await jsRuntime.InvokeAsync<IJSObjectReference>("import", ModulePath);
            if (module is null) return;

            _module = module;
            _dotNetRef = DotNetObjectReference.Create(this);
            _listenerId = await _module.InvokeAsync<int>("addStorageListener", _dotNetRef, storageName);
        }
    }

    internal void InitializeListenerAsync()
    {
        // Fire-and-forget: the Changed event add accessor cannot await. Observe the
        // task so a failed JS module import (prerender, disconnected circuit, blocked
        // storage, missing static asset) is surfaced instead of being swallowed.
        _ = InitializeAsync().AsTask().ContinueWith(
            t => Console.Error.WriteLine(
                $"[D20Tek.Blazor.BrowserStorage] Failed to initialize storage change listener for {storageName}: {t.Exception!.GetBaseException().Message}"),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    [JSInvokable]
    public void OnStorageChanged(string? key, string? oldValue, string? newValue)
    {
        if (key is null) return;

        onChanged(options.StripPrefix(key), oldValue, newValue);
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
