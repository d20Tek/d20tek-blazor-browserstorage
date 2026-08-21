# API Reference

This document provides a complete reference for all public types, interfaces, methods, events, and extension methods in D20Tek.Blazor.BrowserStorage.

## Table of Contents

- [Interfaces](#interfaces)
- [IBrowserStorageService Methods](#ibrowserstorageservice-methods)
- [Bulk Extension Methods](#bulk-extension-methods)
- [Events](#events)
- [DI Registration Methods](#di-registration-methods)
- [Types](#types)

## Interfaces

| Interface | Description |
|---|---|
| `ILocalStorageService` | Provides typed, async access to the browser's `localStorage`. Inherits from `IBrowserStorageService`. |
| `ISessionStorageService` | Provides typed, async access to the browser's `sessionStorage`. Inherits from `IBrowserStorageService`. |
| `IBrowserStorageService` | Base interface defining all storage operations. Implements `IAsyncDisposable`. |

## IBrowserStorageService Methods

| Method | Return Type | Description |
|---|---|---|
| `GetAsync<T>(string key, CancellationToken ct)` | `ValueTask<StorageResult<T>>` | Reads and deserializes a value from storage. Returns a result with `IsSuccess = false` and an `ErrorMessage` when the key is missing, the stored JSON is corrupt, or storage is unavailable. |
| `SetAsync<T>(string key, T value, CancellationToken ct)` | `ValueTask<StorageResult>` | Serializes and writes a value to storage. Returns a result indicating success or failure (e.g., quota exceeded or storage disabled). |
| `RemoveAsync(string key, CancellationToken ct)` | `ValueTask<StorageResult>` | Removes a single key from storage. Returns a result indicating success or failure. |
| `ClearAllAsync(CancellationToken cancellationToken)` | `ValueTask<StorageResult>` | Removes **all** keys from the underlying browser storage area for the current origin. This is a destructive, area-wide operation: it deletes every key in the target storage (`localStorage` or `sessionStorage`), **including keys written by other libraries or application code sharing the same origin**. The configured `KeyPrefix` is **not** honored — to delete only keys owned by this service, enumerate `GetKeysAsync` and call `RemoveAsync` for each. Returns a result indicating success or failure. |
| `ContainsKeyAsync(string key, CancellationToken ct)` | `ValueTask<bool>` | Returns `true` if the specified key exists in storage. |
| `LengthAsync(CancellationToken ct)` | `ValueTask<int>` | Returns the number of keys in storage. |
| `GetKeysAsync(CancellationToken ct)` | `ValueTask<IReadOnlyList<string>>` | Returns a read-only list of all key names in storage. |
| `IsAvailableAsync(CancellationToken ct)` | `ValueTask<bool>` | Returns `true` if the underlying browser storage is available (not blocked, disabled, or in a restricted private mode). The result is cached after the first check. |

## Bulk Extension Methods

The following extension methods are defined in the `BrowserStorageServiceBulkExtensions` static class and are available on any `IBrowserStorageService` instance.

| Method | Return Type | Description |
|---|---|---|
| `SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct)` | `ValueTask<StorageResult>` | Writes multiple key-value pairs to storage. Fails fast: on the first failing write, returns that failure result without attempting the remaining items. |
| `RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct)` | `ValueTask<StorageResult>` | Removes multiple keys from storage. Fails fast on the first failing removal. |

## Events

| Event | Type | Description |
|---|---|---|
| `Changed` | `EventHandler<StorageChangedEventArgs>?` | Raised when a storage value is added, modified, or removed through the service. |

## DI Registration Methods

The following extension methods are defined in the `ServiceCollectionExtensions` static class and are available on any `IServiceCollection` instance.

| Method | Description |
|---|---|
| `AddBrowserStorage(Action<BrowserStorageOptions>?, ServiceLifetime)` | Registers both `ILocalStorageService` and `ISessionStorageService`. |
| `AddLocalStorage(Action<BrowserStorageOptions>?, ServiceLifetime)` | Registers only `ILocalStorageService`. |
| `AddSessionStorage(Action<BrowserStorageOptions>?, ServiceLifetime)` | Registers only `ISessionStorageService`. |

All registration methods accept an optional `Action<BrowserStorageOptions>` delegate for configuration and an optional `ServiceLifetime` parameter that defaults to `ServiceLifetime.Scoped`.

## Types

### StorageResult\<T\>

A readonly record struct returned by `GetAsync<T>` that represents the outcome of a storage read operation.

| Property | Type | Description |
|---|---|---|
| `IsSuccess` | `bool` | Indicates whether the key was found and the value was deserialized successfully. |
| `Value` | `T?` | The deserialized value, or the default value of `T` if the read did not succeed. |
| `ErrorMessage` | `string?` | A description of the failure when `IsSuccess` is `false` (e.g., key not found, corrupt JSON, or storage unavailable). `null` on success. |

### StorageResult

A readonly record struct returned by write and remove operations (`SetAsync`, `RemoveAsync`, `ClearAllAsync`, `SetMultipleAsync`, `RemoveMultipleAsync`) that represents the outcome of a mutation.

| Property | Type | Description |
|---|---|---|
| `IsSuccess` | `bool` | Indicates whether the operation completed successfully. |
| `ErrorMessage` | `string?` | A description of the failure when `IsSuccess` is `false` (e.g., quota exceeded, JS interop error, or storage unavailable). `null` on success. |

| Member | Type | Description |
|---|---|---|
| `StorageResult.Success()` | `StorageResult` | Creates a successful result with no error message. |
| `StorageResult.Failure(string errorMessage)` | `StorageResult` | Creates a failure result with the specified error message. |

### StorageChangedEventArgs

Event arguments provided by the `Changed` event when a storage value is modified.

| Property | Type | Description |
|---|---|---|
| `Key` | `string` | The storage key that was modified. |
| `OldValue` | `object?` | The previous value, or null if the key is new. |
| `NewValue` | `object?` | The new value, or null if the key was removed. |

### BrowserStorageOptions

Configuration class used to customize storage service behavior during dependency injection registration.

| Property | Type | Default | Description |
|---|---|---|---|
| `KeyPrefix` | `string` | `""` (empty) | A prefix string that is automatically prepended to all storage keys. |
| `JsonOptions` | `JsonSerializerOptions?` | `null` | Custom JSON serializer options. When null, the library uses `JsonSerializerDefaults.Web`. |

## Trimming and AOT

`GetAsync<T>` and `SetAsync<T>` (and by extension `SetMultipleAsync`) are annotated with `[RequiresUnreferencedCode]` and `[RequiresDynamicCode]`. For types outside the built-in primitive set, they fall back to reflection-based `System.Text.Json`, which is not compatible with:

- Blazor WebAssembly AOT compilation (`<RunAOTCompilation>true</RunAOTCompilation>`)
- Full trim mode (`<TrimMode>full</TrimMode>`) without root descriptors for your stored types
- Native AOT

To use the library from an AOT- or trim-enabled project, configure `BrowserStorageOptions.JsonOptions.TypeInfoResolver` with a source-generated `JsonSerializerContext` covering the types you store:

```csharp
[JsonSerializable(typeof(UserPreferences))]
[JsonSerializable(typeof(HighScore))]
internal partial class AppJsonContext : JsonSerializerContext { }

services.AddLocalStorage(options =>
{
    options.JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = AppJsonContext.Default,
    };
});
```

Consumers who don't publish with trimming/AOT can suppress the warnings with `[UnconditionalSuppressMessage("Trimming", "IL2026")]` at their call sites, or ignore them — the library will continue to use reflection at runtime.
