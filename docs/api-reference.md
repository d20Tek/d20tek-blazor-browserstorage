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
| `GetAsync<T>(string key, CancellationToken ct)` | `ValueTask<StorageResult<T>>` | Reads and deserializes a value from storage. Returns a result with `IsSuccess = false` if the key is not found. |
| `SetAsync<T>(string key, T value, CancellationToken ct)` | `ValueTask` | Serializes and writes a value to storage. |
| `RemoveAsync(string key, CancellationToken ct)` | `ValueTask` | Removes a single key from storage. |
| `ClearAsync(CancellationToken ct)` | `ValueTask` | Removes all keys from storage. |
| `ContainsKeyAsync(string key, CancellationToken ct)` | `ValueTask<bool>` | Returns `true` if the specified key exists in storage. |
| `LengthAsync(CancellationToken ct)` | `ValueTask<int>` | Returns the number of keys in storage. |
| `GetKeysAsync(CancellationToken ct)` | `ValueTask<IReadOnlyList<string>>` | Returns a read-only list of all key names in storage. |

## Bulk Extension Methods

The following extension methods are defined in the `BrowserStorageServiceBulkExtensions` static class and are available on any `IBrowserStorageService` instance.

| Method | Return Type | Description |
|---|---|---|
| `SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct)` | `ValueTask` | Writes multiple key-value pairs to storage. |
| `RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct)` | `ValueTask` | Removes multiple keys from storage. |

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
| `Value` | `T?` | The deserialized value, or the default value of `T` if the key was not found. |

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
