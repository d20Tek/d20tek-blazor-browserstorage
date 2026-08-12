# D20Tek.Blazor.BrowserStorage — Design & Implementation Plan

## Overview

A modern, lightweight browser storage library for Blazor WebAssembly and client-side Blazor render modes. Provides typed access to `localStorage` and `sessionStorage` via clean async APIs and `IJSRuntime` interop. Zero JS file dependencies — all interop is inline.

## Package Identity

| Field | Value |
|-------|-------|
| Package ID | `D20Tek.Blazor.BrowserStorage` |
| Repository | `github.com/d20Tek/d20tek-blazor-browserstorage` |
| License | MIT |
| Target Frameworks | `net9.0`, `net10.0` |
| Dependencies | `Microsoft.AspNetCore.Components.WebAssembly` (for `IJSRuntime`) |
| Tags | blazor, wasm, webassembly, localstorage, sessionstorage, browser, storage |
| Description | Modern browser storage (localStorage + sessionStorage) for Blazor WebAssembly and client-side render modes. Typed, async, lightweight. |

---

## Target Audience

- Blazor WebAssembly developers
- Blazor InteractiveAuto developers (client-side rendering)
- Developers migrating from deprecated Blazored.LocalStorage
- Anyone needing typed key-value browser storage without server dependency

---

## v1.0 Features

### Core
- **Local Storage** — persistent key-value storage (survives browser restart)
- **Session Storage** — tab-scoped key-value storage (cleared on tab close)
- **Typed API** — `GetAsync<T>` / `SetAsync<T>` with `System.Text.Json` serialization
- **Result-based reads** — returns a result struct to handle missing keys without exceptions
- **CancellationToken support** — all async methods accept optional cancellation
- **Public Api documentation** — XML comments for all public types and members

### Extras
- **Key prefix/namespace** — configurable prefix to avoid collisions across apps on same origin
- **Storage changed event** — observable callback when values change (cross-tab via `StorageEvent`)
- **Bulk operations** — `SetMultipleAsync`, `RemoveMultipleAsync` to minimize JS interop round-trips
- **Key enumeration** — `GetKeysAsync`, `LengthAsync`

### DI Registration
- `AddBrowserStorage()` — registers both local and session storage services
- `AddLocalStorage()` / `AddSessionStorage()` — register individually
- Configurable via `BrowserStorageOptions`

---

## Public API

### Result Type

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public readonly record struct StorageResult<T>(bool Success, T? Value);
```

### Core Interface

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public interface IBrowserStorageService
{
	ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken ct = default);

	ValueTask SetAsync<T>(string key, T value, CancellationToken ct = default);

	ValueTask RemoveAsync(string key, CancellationToken ct = default);

	ValueTask ClearAsync(CancellationToken ct = default);

	ValueTask<bool> ContainsKeyAsync(string key, CancellationToken ct = default);

	ValueTask<int> LengthAsync(CancellationToken ct = default);

	ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken ct = default);

	ValueTask SetMultipleAsync(IEnumerable<KeyValuePair<string, object>> items, CancellationToken ct = default);

	ValueTask RemoveMultipleAsync(IEnumerable<string> keys, CancellationToken ct = default);

	event EventHandler<StorageChangedEventArgs>? Changed;
}
```

### Derived Interfaces

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public interface ILocalStorageService : IBrowserStorageService;

public interface ISessionStorageService : IBrowserStorageService;
```

### Event Args

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public sealed class StorageChangedEventArgs(string key, object? oldValue, object? newValue) : EventArgs
{
	public string Key { get; } = key;
	public object? OldValue { get; } = oldValue;
	public object? NewValue { get; } = newValue;
}
```

### Options

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public sealed class BrowserStorageOptions
{
	/// <summary>
	/// Prefix prepended to all keys (e.g., "myapp_"). Default: empty string.
	/// </summary>
	public string KeyPrefix { get; set; } = string.Empty;

	/// <summary>
	/// JSON serializer options for value serialization.
	/// Default: System.Text.Json web defaults (camelCase, case-insensitive reads).
	/// </summary>
	public JsonSerializerOptions? JsonOptions { get; set; }
}
```

### DI Extensions

```csharp
namespace D20Tek.Blazor.BrowserStorage;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddBrowserStorage(
		this IServiceCollection services,
		Action<BrowserStorageOptions>? configure = null);

	public static IServiceCollection AddLocalStorage(
		this IServiceCollection services,
		Action<BrowserStorageOptions>? configure = null);

	public static IServiceCollection AddSessionStorage(
		this IServiceCollection services,
		Action<BrowserStorageOptions>? configure = null);
}
```

---

## Usage Examples

### Registration

```csharp
// Program.cs
builder.Services.AddBrowserStorage(options =>
{
	options.KeyPrefix = "fortuna_";
});

// Or register individually
builder.Services.AddLocalStorage();
builder.Services.AddSessionStorage();
```

### Reading and Writing

```csharp
@inject ILocalStorageService LocalStorage

// Write
await LocalStorage.SetAsync("showClosed", true);
await LocalStorage.SetAsync("userPrefs", new UserPrefs { Theme = "dark" });

// Read
var result = await LocalStorage.GetAsync<bool>("showClosed");
var showClosed = result.Success ? result.Value : false;

// Read complex type
var prefsResult = await LocalStorage.GetAsync<UserPrefs>("userPrefs");
if (prefsResult.Success)
{
	// use prefsResult.Value
}
```

### Key Management

```csharp
var exists = await LocalStorage.ContainsKeyAsync("showClosed");
var count = await LocalStorage.LengthAsync();
var keys = await LocalStorage.GetKeysAsync();

await LocalStorage.RemoveAsync("showClosed");
await LocalStorage.ClearAsync();
```

### Bulk Operations

```csharp
await LocalStorage.SetMultipleAsync(new Dictionary<string, object>
{
	["key1"] = "value1",
	["key2"] = 42,
	["key3"] = new MyObject()
});

await LocalStorage.RemoveMultipleAsync(["key1", "key2", "key3"]);
```

### Change Events

```csharp
LocalStorage.Changed += (sender, e) =>
{
	Console.WriteLine($"Key '{e.Key}' changed from {e.OldValue} to {e.NewValue}");
};
```

---

## Internal Architecture

### Project Structure

```
d20tek-blazor-browserstorage/
├── src/
│   └── D20Tek.Blazor.BrowserStorage/
│       ├── D20Tek.Blazor.BrowserStorage.csproj
│       ├── IBrowserStorageService.cs
│       ├── ILocalStorageService.cs
│       ├── ISessionStorageService.cs
│       ├── StorageResult.cs
│       ├── StorageChangedEventArgs.cs
│       ├── BrowserStorageOptions.cs
│       ├── ServiceCollectionExtensions.cs
│       └── Internal/
│           ├── LocalStorageService.cs
│           ├── SessionStorageService.cs
│           └── JsInterop.cs
├── tests/
│   └── D20Tek.Blazor.BrowserStorage.Tests/
│       ├── D20Tek.Blazor.BrowserStorage.Tests.csproj
│       ├── LocalStorageServiceTests.cs
│       ├── SessionStorageServiceTests.cs
│       ├── StorageResultTests.cs
│       ├── BrowserStorageOptionsTests.cs
│       └── Fakes/
│           └── FakeJSRuntime.cs
├── samples/
│   └── SampleApp/
│       └── (Blazor WASM app demonstrating all features)
├── README.md
├── LICENSE
├── CHANGELOG.md
└── D20Tek.Blazor.BrowserStorage.sln
```

### JS Interop Strategy

No external `.js` file. All calls via inline `IJSRuntime`:

```csharp
// Get
await js.InvokeAsync<string?>("localStorage.getItem", key);

// Set
await js.InvokeVoidAsync("localStorage.setItem", key, json);

// Remove
await js.InvokeVoidAsync("localStorage.removeItem", key);

// Clear
await js.InvokeVoidAsync("localStorage.clear");

// Length
await js.InvokeAsync<int>("eval", "localStorage.length");

// Key at index (for enumeration)
await js.InvokeAsync<string?>("localStorage.key", index);
```

For `sessionStorage`, substitute `sessionStorage` for `localStorage`.

### Serialization Strategy

- Use `System.Text.Json` with configurable `JsonSerializerOptions`
- Primitives (`string`, `int`, `bool`, `decimal`) stored as-is without JSON wrapping
- Complex types serialized to JSON string
- Null values remove the key (or store literal `"null"` — TBD, document behavior)

### Key Prefixing

All methods internally prepend `BrowserStorageOptions.KeyPrefix`:

```csharp
private string PrefixKey(string key) => $"{_options.KeyPrefix}{key}";
```

`GetKeysAsync` strips the prefix before returning to callers.

### StorageEvent Listener

Register a JS `storage` event listener for cross-tab change detection:

```javascript
window.addEventListener('storage', (e) => {
	DotNet.invokeMethodAsync('D20Tek.Blazor.BrowserStorage', 'OnStorageChanged', 
		e.key, e.oldValue, e.newValue);
});
```

This requires a small inline script registered during service initialization via `IJSRuntime`.

---

## Implementation Steps

| # | Step | Deliverable | Status |
|---|------|-------------|--------|
| 1 | Create repo, solution, and project structure | `.sln`, `.csproj` files, folder layout | Done |
| 2 | Define public API contracts | All interfaces, records, options, event args | Done |
| 3 | Implement `JsInterop` helper | Internal static class wrapping `IJSRuntime` calls | Done |
| 4 | Implement `LocalStorageService` | Full `ILocalStorageService` implementation | Done |
| 5 | Implement `SessionStorageService` | Full `ISessionStorageService` implementation | Done |
| 6 | Implement `StorageEvent` listener | Cross-tab change detection + `Changed` event | Done |
| 7 | Implement DI extensions | `AddBrowserStorage`, `AddLocalStorage`, `AddSessionStorage` | Done |
| 8 | Unit tests | Test all methods with mocked `IJSRuntime` | Done |
| 9 | Sample app — Theme & Preferences | Blazor WASM app demonstrating basic usage with primitives | |
| 10 | Sample app — Quiz/Trivia | Blazor WASM app demonstrating complex types and full feature set | |
| 11 | Documentation | README, API docs, migration guide from Blazored | |
| 12 | CI/CD | GitHub Actions for build, test, NuGet publish | |
| 13 | Publish | NuGet package, GitHub release | |

---

## Sample Apps

### Sample 1: Theme & Preferences Dashboard

A simple settings page demonstrating basic primitive storage and cross-tab sync.

**localStorage (persists across sessions):**
- Preferred theme (dark/light) — `string`
- Accent color — `string`
- Font size — `int`

**sessionStorage (resets on tab close):**
- Unsaved settings draft flag — `bool`
- "What's new" banner dismissed — `bool`

**Features demonstrated:**
- `GetAsync<T>` / `SetAsync<T>` with primitive types (string, int, bool)
- `ContainsKeyAsync` for first-visit detection
- `Changed` event for cross-tab theme synchronization
- `RemoveAsync` for resetting individual preferences
- Key prefix namespacing (`theme_`, `session_`)
- Result-based reads for missing keys with fallback defaults

**Pages:**
- Settings page with live preview of theme/accent/font changes
- "Reset to defaults" button demonstrating `ClearAsync`

---

### Sample 2: Quiz/Trivia App

An interactive quiz demonstrating complex typed objects, bulk operations, and full feature coverage.

**localStorage (persists across sessions):**
- Player profile — complex object (`{ Name, AvatarUrl, CreatedDate }`)
- High scores list — `List<ScoreEntry>` with `{ Score, Date, Category }`
- Total games played — `int`
- Categories unlocked — `List<string>`

**sessionStorage (resets on tab close):**
- Current quiz state — complex object (`{ QuestionIndex, SelectedAnswers, Category, StartTime }`)
- Timer remaining — `int` (seconds)
- Current streak — `int`

**Features demonstrated:**
- Typed complex objects with `System.Text.Json` serialization
- `StorageResult<T>` for handling first-time players (no saved profile)
- `SetMultipleAsync` for saving quiz results (score + updated stats in one batch)
- `RemoveMultipleAsync` for clearing session state on quiz completion
- `GetKeysAsync` / `LengthAsync` for displaying saved data stats
- `Changed` event for cross-tab "new high score" toast notification
- `DateTimeOffset` and `TimeSpan` serialization for timestamps and durations

**Pages:**
- Home — player name entry or welcome back (result-based read)
- Category selection — shows unlocked vs locked categories
- Quiz — timed questions with session-state recovery on refresh
- Results — score display, high score check, bulk save
- Leaderboard — enumerated scores from localStorage

---

## Non-goals for v1

- ❌ IndexedDB support (planned for v2)
- ❌ Client-side encryption (planned for v2 as `D20Tek.Blazor.BrowserStorage.Protected`)
- ❌ Synchronous API (JS interop is async-only in WASM)
- ❌ Server-side Blazor support (use Microsoft's `ProtectedLocalStorage` for that)
- ❌ .NET 8 support (focus on current LTS + latest)

---

## Future Roadmap (v2+)

- **IndexedDB support** — `IIndexedDbService` for larger structured data
- **Encrypted storage** — `IProtectedLocalStorageService` with client-side AES
- **TTL/Expiration** — optional time-to-live per key
- **Compression** — optional gzip for large values
- **Source-generated serialization** — AOT-friendly `JsonSerializerContext` support

---

## Migration Guide (from Blazored.LocalStorage)

| Blazored | D20Tek.Blazor.BrowserStorage |
|----------|------------------------------|
| `ILocalStorageService.GetItemAsync<T>(key)` | `ILocalStorageService.GetAsync<T>(key)` → check `.Success` |
| `ILocalStorageService.SetItemAsync(key, value)` | `ILocalStorageService.SetAsync(key, value)` |
| `ILocalStorageService.RemoveItemAsync(key)` | `ILocalStorageService.RemoveAsync(key)` |
| `ILocalStorageService.ClearAsync()` | `ILocalStorageService.ClearAsync()` |
| `ILocalStorageService.ContainKeyAsync(key)` | `ILocalStorageService.ContainsKeyAsync(key)` |
| `ILocalStorageService.LengthAsync()` | `ILocalStorageService.LengthAsync()` |
| `builder.Services.AddBlazoredLocalStorage()` | `builder.Services.AddBrowserStorage()` |
| Throws on missing key | Returns `StorageResult<T>` with `Success = false` |
