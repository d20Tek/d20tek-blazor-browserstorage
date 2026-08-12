using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class SessionStorageService(IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    : WebStorageService("sessionStorage", jsRuntime, options), ISessionStorageService;
