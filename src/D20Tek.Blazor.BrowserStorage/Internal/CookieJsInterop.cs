namespace D20Tek.Blazor.BrowserStorage.Internal;

/// <summary>
/// Internal helper that lazily imports the BrowserStorageInterop.js module and exposes
/// the cookie-specific JS interop functions.
/// </summary>
internal sealed class CookieJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private const string ModulePath = "./_content/D20Tek.Blazor.BrowserStorage/BrowserStorageInterop.js";
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IJSObjectReference? _module;

    private async ValueTask<IJSObjectReference> GetModuleAsync(CancellationToken ct)
    {
        if (_module is not null) return _module;

        await _gate.WaitAsync(ct);
        try
        {
            _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", ct, ModulePath);
            return _module;
        }
        finally
        {
            _gate.Release();
        }
    }

    public async ValueTask<string?> GetCookieAsync(string key, CancellationToken ct)
    {
        var module = await GetModuleAsync(ct);
        return await module.InvokeAsync<string?>("getCookie", ct, key);
    }

    public async ValueTask SetCookieAsync(
        string key,
        string value,
        long? maxAgeSeconds,
        string path,
        string? domain,
        string sameSite,
        bool secure,
        CancellationToken ct)
    {
        var module = await GetModuleAsync(ct);
        await module.InvokeVoidAsync("setCookie", ct, key, value, maxAgeSeconds, path, domain, sameSite, secure);
    }

    public async ValueTask RemoveCookieAsync(string key, string path, string? domain, CancellationToken ct)
    {
        var module = await GetModuleAsync(ct);
        await module.InvokeVoidAsync("removeCookie", ct, key, path, domain);
    }

    public async ValueTask<IReadOnlyList<string>> GetAllCookieKeysAsync(CancellationToken ct)
    {
        var module = await GetModuleAsync(ct);
        return await module.InvokeAsync<IReadOnlyList<string>>("getAllCookieKeys", ct);
    }

    public async ValueTask<bool> IsAvailableAsync(CancellationToken ct)
    {
        try
        {
            var module = await GetModuleAsync(ct);
            return await module.InvokeAsync<bool>("isCookieAvailable", ct);
        }
        catch (Exception ex)
        {
            var reason = ex switch
            {
                JSDisconnectedException => "Blazor Server circuit disconnected",
                InvalidOperationException => "Blazor Server prerendering",
                _ => "Unexpected error",
            };
            Console.WriteLine($"{reason}. Treating cookie storage as unavailable.");
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already torn down; nothing to clean up.
            }
        }

        _gate.Dispose();
    }
}
