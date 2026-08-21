namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class StorageAvailabilityGate(string storageName, IJSRuntime jsRuntime)
{
    private readonly string _storageName = storageName;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private Lazy<Task<bool>>? _check;

    public ValueTask<bool> IsAvailableAsync(CancellationToken ct)
    {
        var lazy = LazyInitializer.EnsureInitialized(
            ref _check,
            () => new Lazy<Task<bool>>(ProbeAsync, LazyThreadSafetyMode.ExecutionAndPublication));
        return new ValueTask<bool>(lazy.Value.WaitAsync(ct));
    }

    private async Task<bool> ProbeAsync()
    {
        try
        {
            return await JsInterop.IsStorageAvailableAsync(_jsRuntime, _storageName, CancellationToken.None)
                                  .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            var reason = ex switch
            {
                JSDisconnectedException => "Blazor Server circuit disconnected",
                InvalidOperationException => "Blazor Server prerendering",
                _ => "Unexpected error",
            };
            Console.WriteLine($"{reason}. Treating storage '{_storageName}' as unavailable.");
            return false;
        }
    }
}
