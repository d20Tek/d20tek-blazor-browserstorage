using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace D20Tek.Blazor.BrowserStorage.Internal;

internal sealed class LocalStorageService(IJSRuntime jsRuntime, IOptions<BrowserStorageOptions> options)
    : WebStorageService("localStorage", jsRuntime, options), ILocalStorageService;
