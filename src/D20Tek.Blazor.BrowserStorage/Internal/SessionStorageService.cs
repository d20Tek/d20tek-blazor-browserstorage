namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class SessionStorageService(IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    : WebStorageService("sessionStorage", jsRuntime, options), ISessionStorageService;
