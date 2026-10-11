# Testing API Reference

This document provides a complete reference for all public types, interfaces, methods, events, and extension methods in **D20Tek.Blazor.BrowserStorage.Testing** - the companion package for unit- and component-testing code that depends on `ILocalStorageService` or `ISessionStorageService`.

## Table of Contents

- [Interfaces](#interfaces)
- [IInMemoryStorage Members](#iinmemorystorage-members)
- [In-Memory Service Classes](#in-memory-service-classes)
- [IServiceCollection Extension Methods](#iservicecollection-extension-methods)
- [IServiceProvider Extension Methods](#iserviceprovider-extension-methods)
- [IBrowserStorageService Extension Methods](#ibrowserstorageservice-extension-methods)
- [bUnit BunitContext Extension Methods](#bunit-bunitcontext-extension-methods)

## Interfaces

| Interface | Description |
|---|---|
| `IInMemoryStorage` | Test-time surface for arranging, inspecting, and manipulating the in-memory backing store. Implemented by `InMemoryLocalStorageService`, `InMemorySessionStorageService`, and `InMemoryCookieStorageService`. |

## IInMemoryStorage Members

| Member | Return Type | Description |
|---|---|---|
| `Snapshot` | `IReadOnlyDictionary<string, string>` | Returns a read-only view of the underlying dictionary. Keys appear in their prefixed form, matching what the real browser storage would see. |
| `Seed(string key, string rawJson)` | `void` | Writes a raw serialized value directly to the store without raising the `Changed` event. The configured `KeyPrefix` is applied automatically. |
| `Seed<T>(string key, T value)` | `void` | Serializes and writes a value silently (no `Changed` event). Uses the same `StorageSerializer` and `JsonOptions` as the real service. |
| `Seed(IEnumerable<KeyValuePair<string, string>> entries)` | `void` | Bulk silent-seed of raw JSON entries. |
| `Seed<T>(IEnumerable<KeyValuePair<string, T>> entries)` | `void` | Bulk silent-seed of typed entries. |
| `Clear()` | `void` | Synchronously removes every key from the store without raising `Changed`. Intended for test setup and teardown. |
| `SimulateUnavailable()` | `void` | Marks the store as unavailable. Subsequent `GetAsync`, `SetAsync`, `RemoveAsync`, and `ClearAllAsync` calls return failure `StorageResult`s, and `ContainsKeyAsync` / `LengthAsync` / `GetKeysAsync` return empty results - mirroring the real service when the browser blocks storage. |
| `RestoreAvailable()` | `void` | Restores availability after a previous `SimulateUnavailable()` call. |
| `RaiseExternalChange(string key, string? oldValue, string? newValue)` | `void` | Fires the `Changed` event as if another browser tab had modified storage. `oldValue` / `newValue` are the raw serialized JSON strings (matching what the real service stores). Does not mutate the store. |

## In-Memory Service Classes

Both classes implement their production interface, `IInMemoryStorage`, and `IDisposable` / `IAsyncDisposable`. Each class exposes three constructors so it can be used with or without DI.

### InMemoryLocalStorageService

Implements `ILocalStorageService`. Backed by an in-memory `ConcurrentDictionary<string, string>` that mirrors the real service's serialization, key-prefixing, `StorageResult`, and `Changed`-event semantics.

| Constructor | Description |
|---|---|
| `InMemoryLocalStorageService()` | Uses default `BrowserStorageOptions` (no key prefix). |
| `InMemoryLocalStorageService(BrowserStorageOptions options)` | Uses the supplied options directly. |
| `InMemoryLocalStorageService(IOptions<BrowserStorageOptions> options)` | DI-friendly constructor used by the `IServiceCollection` extensions. |

### InMemorySessionStorageService

Implements `ISessionStorageService`. Constructor set matches `InMemoryLocalStorageService`.

| Constructor | Description |
|---|---|
| `InMemorySessionStorageService()` | Uses default `BrowserStorageOptions`. |
| `InMemorySessionStorageService(BrowserStorageOptions options)` | Uses the supplied options directly. |
| `InMemorySessionStorageService(IOptions<BrowserStorageOptions> options)` | DI-friendly constructor used by the `IServiceCollection` extensions. |

### InMemoryCookieStorageService

Implements `ICookieStorageService`. Constructor set matches `InMemoryLocalStorageService`. This fake mirrors the key/value and change-notification semantics shared with the other in-memory services; it does not model cookie-specific attributes (path, domain, SameSite, Secure, expiration) since those are not observable through `IBrowserStorageService`.

| Constructor | Description |
|---|---|
| `InMemoryCookieStorageService()` | Uses default `BrowserStorageOptions`. |
| `InMemoryCookieStorageService(BrowserStorageOptions options)` | Uses the supplied options directly. |
| `InMemoryCookieStorageService(IOptions<BrowserStorageOptions> options)` | DI-friendly constructor used by the `IServiceCollection` extensions. |

## IServiceCollection Extension Methods

The following extension methods are defined in the `InMemoryBrowserStorageServiceCollectionExtensions` static class and are available on any `IServiceCollection` instance. They are framework-agnostic and work with xUnit, MSTest, NUnit, `WebApplicationFactory`, and any other DI-based test host.

| Method | Description |
|---|---|
| `AddInMemoryBrowserStorage(Action<BrowserStorageOptions>?)` | Registers both `ILocalStorageService` and `ISessionStorageService` as singletons backed by the in-memory implementations, plus keyed `IInMemoryStorage` handles for arrange / assert. |
| `AddInMemoryLocalStorage(Action<BrowserStorageOptions>?)` | Registers only the in-memory `ILocalStorageService` and its keyed `IInMemoryStorage` handle. |
| `AddInMemorySessionStorage(Action<BrowserStorageOptions>?)` | Registers only the in-memory `ISessionStorageService` and its keyed `IInMemoryStorage` handle. |
| `ReplaceWithInMemoryBrowserStorage(Action<BrowserStorageOptions>?)` | Removes any prior `ILocalStorageService` and `ISessionStorageService` registrations (including those added by the production `AddBrowserStorage`) and registers the in-memory implementations in their place. Intended for integration-test setups such as `WebApplicationFactory`. |
| `ReplaceWithInMemoryLocalStorage(Action<BrowserStorageOptions>?)` | Replaces only the local storage registration. |
| `ReplaceWithInMemorySessionStorage(Action<BrowserStorageOptions>?)` | Replaces only the session storage registration. |

All registration methods accept an optional `Action<BrowserStorageOptions>` delegate for configuration. Services are registered with singleton lifetime.

The following extension methods are defined in the `InMemoryCookieStorageServiceCollectionExtensions` static class and are available on any `IServiceCollection` instance.

| Method | Description |
|---|---|
| `AddInMemoryCookieStorage(Action<BrowserStorageOptions>?)` | Registers the in-memory `ICookieStorageService` as a singleton, plus a keyed `IInMemoryStorage` handle for arrange / assert. |
| `ReplaceWithInMemoryCookieStorage(Action<BrowserStorageOptions>?)` | Removes any prior `ICookieStorageService` registration (including the one added by the production `AddCookieStorage`) and registers the in-memory implementation in its place. |

## IServiceProvider Extension Methods

Resolve the test-time handles for arrange / assert operations after building the service provider.

| Method | Return Type | Description |
|---|---|---|
| `GetInMemoryLocalStorage()` | `IInMemoryStorage` | Resolves the in-memory local storage handle. Throws if `AddInMemoryLocalStorage` (or `AddInMemoryBrowserStorage`) was not called. |
| `GetInMemorySessionStorage()` | `IInMemoryStorage` | Resolves the in-memory session storage handle. Throws if `AddInMemorySessionStorage` (or `AddInMemoryBrowserStorage`) was not called. |
| `GetInMemoryCookieStorage()` | `IInMemoryStorage` | Resolves the in-memory cookie storage handle. Throws if `AddInMemoryCookieStorage` was not called. |

## IBrowserStorageService Extension Methods

The following extension method is defined in the `BrowserStorageChangeRecorderExtensions` static class and works with any `IBrowserStorageService` - both the in-memory services and the real `WebStorageService`.

| Method | Return Type | Description |
|---|---|---|
| `RecordChanges(out IReadOnlyList<StorageChangedEventArgs> events)` | `IDisposable` | Starts recording `Changed` events raised by the service. The captured events are exposed through the `events` out parameter as a thread-safe, read-only list that is populated live. Dispose the returned handle (or leave it to a `using` block) to stop recording. |

## bUnit BunitContext Extension Methods

The following extension methods are defined in the `BrowserStorageTestContextExtensions` static class and are available on any bUnit `BunitContext`.

| Method | Return Type | Description |
|---|---|---|
| `AddBrowserStorage(Action<BrowserStorageOptions>?)` | `BunitContext` | Registers both the in-memory local and session storage services on the context's `Services`. Returns the context for chaining. |
| `AddLocalStorage(Action<BrowserStorageOptions>?)` | `BunitContext` | Registers only the in-memory local storage service. |
| `AddSessionStorage(Action<BrowserStorageOptions>?)` | `BunitContext` | Registers only the in-memory session storage service. |
| `AddCookieStorage(Action<BrowserStorageOptions>?)` | `BunitContext` | Registers only the in-memory cookie storage service. |
| `GetLocalStorage()` | `IInMemoryStorage` | Resolves the in-memory local storage handle from the context's `Services`. |
| `GetSessionStorage()` | `IInMemoryStorage` | Resolves the in-memory session storage handle from the context's `Services`. |
| `GetCookieStorage()` | `IInMemoryStorage` | Resolves the in-memory cookie storage handle from the context's `Services`. |

## Related documentation

- Getting-started guide: [`docs/testing.md`](testing.md)
- Main library API reference: [`docs/api-reference.md`](api-reference.md)
