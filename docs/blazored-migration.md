# Migration from Blazored.LocalStorage (SessionStorage)

This document provides guidance for developers migrating from the Blazored.LocalStorage package to D20Tek.Blazor.BrowserStorage. The two libraries share a similar API surface, but there are several important differences in method naming, return types, and registration.

## Method Mapping

The following table maps Blazored.LocalStorage methods to their D20Tek.Blazor.BrowserStorage equivalents:

| Blazored.LocalStorage | D20Tek.Blazor.BrowserStorage | Notes |
|---|---|---|
| `ILocalStorageService.GetItemAsync<T>(key)` | `ILocalStorageService.GetAsync<T>(key)` | Returns `StorageResult<T>` instead of throwing on missing keys. Check `.IsSuccess` before accessing `.Value`. |
| `ILocalStorageService.SetItemAsync(key, value)` | `ILocalStorageService.SetAsync(key, value)` | Returns `StorageResult`. Check `.IsSuccess` to detect quota/blocked-storage failures. |
| `ILocalStorageService.RemoveItemAsync(key)` | `ILocalStorageService.RemoveAsync(key)` | Returns `StorageResult`. |
| `ILocalStorageService.ClearAsync()` | `ILocalStorageService.ClearAllAsync()` | Returns `StorageResult`. Renamed to reflect that it clears the entire browser storage area, not just prefixed keys. |
| `ILocalStorageService.ContainKeyAsync(key)` | `ILocalStorageService.ContainsKeyAsync(key)` | Note the corrected method name spelling. |
| `ILocalStorageService.LengthAsync()` | `ILocalStorageService.LengthAsync()` | Identical behavior. |
| `builder.Services.AddBlazoredLocalStorage()` | `builder.Services.AddBrowserStorage()` | Also registers `ISessionStorageService`. Use `AddLocalStorage()` for localStorage only. |
| Throws on missing key | Returns `StorageResult<T>` with `IsSuccess = false` | No exception handling required for missing keys. |

Note: Blazored.SessionStorage has the same API as Blazored.LocalStorage, but is a separate package, so the method mapping above applies to both libraries. D20Tek.Blazor.BrowserStorage just provides both localStorage and sessionStorage support in one package.

## Key Differences

### Result-based reads and writes

The most significant difference between the two libraries is how failures are surfaced. Blazored.LocalStorage throws an exception when `GetItemAsync<T>` is called with a key that does not exist, and write failures propagate as raw JS interop exceptions. D20Tek.Blazor.BrowserStorage returns a result type for both reads and mutations, so callers can handle missing keys, corrupt data, and quota/blocked-storage errors without exception handling.

**Before (Blazored):**

```csharp
try
{
	var theme = await localStorage.GetItemAsync<string>("theme");
}
catch (Exception)
{
	var theme = "light"; // fallback
}
```

**After (D20Tek.Blazor.BrowserStorage):**

```csharp
var result = await localStorage.GetAsync<string>("theme");
var theme = result.IsSuccess ? result.Value : "light";

var writeResult = await localStorage.SetAsync("theme", theme);
if (!writeResult.IsSuccess)
{
	logger.LogWarning("Storage write failed: {Error}", writeResult.ErrorMessage);
}
```

### Session storage support

Blazored.LocalStorage provides access to `localStorage` only (`sessionStorage` was a whole separate package). D20Tek.Blazor.BrowserStorage includes both `ILocalStorageService` and `ISessionStorageService`, allowing applications to use session-scoped storage for temporary data that should not persist beyond the current browser tab.

### Registration

Replace the Blazored registration call with the D20Tek equivalent:

**Before:**

```csharp
builder.Services.AddBlazoredLocalStorage();
```

**After:**

```csharp
builder.Services.AddBrowserStorage();
```

If you only need localStorage (to match the Blazored feature set), you can use:

```csharp
builder.Services.AddLocalStorage();
```

### Additional features

D20Tek.Blazor.BrowserStorage includes several features not available in Blazored.LocalStorage:

- **Key prefix namespacing** via `BrowserStorageOptions.KeyPrefix`
- **Availability probing** via `IsAvailableAsync` to detect blocked or disabled storage
- **Result-based writes** — `SetAsync`, `RemoveAsync`, and `ClearAllAsync` return `StorageResult` so quota / blocked-storage failures are non-throwing
- **Bulk operations** via `SetMultipleAsync` and `RemoveMultipleAsync` (fail-fast, returning a `StorageResult`)
- **Change notifications** via the `Changed` event
- **Configurable DI service lifetimes** (Scoped, Singleton, or Transient)
- **Custom JSON serialization options** via `BrowserStorageOptions.JsonOptions`

For detailed documentation on these features, see the [Detailed Getting Started Guide](getting-started-detailed.md) guide.
