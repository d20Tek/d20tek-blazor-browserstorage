# Testing components that use D20Tek.Blazor.BrowserStorage

The `D20Tek.Blazor.BrowserStorage.Testing` package lets you unit- and component-test code that depends on `ILocalStorageService` or `ISessionStorageService` without spinning up a real browser or mocking `IJSRuntime` by hand.

It provides:

- Real-behavior in-memory implementations of `ILocalStorageService` and `ISessionStorageService` that route through the same serializer, key-prefix rules, and `StorageResult` semantics as the production service.
- A test-time `IInMemoryStorage` handle for seeding data (single or bulk), clearing the store, inspecting a snapshot, simulating unavailability, and raising cross-tab `Changed` events.
- A `RecordChanges` extension on `IBrowserStorageService` that captures `Changed` events without boilerplate - works with the in-memory services and the real service.
- Framework-agnostic `IServiceCollection` extensions (`AddInMemoryBrowserStorage`, `ReplaceWithInMemoryBrowserStorage`, and matching local/session-only variants) for xUnit, MSTest, NUnit, `WebApplicationFactory`, and any other DI-based test host.
- bUnit `BunitContext` extension methods that wire everything up in a single line.

## Install

```bash
dotnet add package D20Tek.Blazor.BrowserStorage.Testing
```

The package targets `net9.0` and `net10.0` and references bUnit 2.x.

## Getting started

The package supports three usage tiers, from most direct to most integrated:

### Pure unit test - no DI

Instantiate the service directly using either the parameterless constructor or one that accepts `BrowserStorageOptions`:

```csharp
using D20Tek.Blazor.BrowserStorage;
using D20Tek.Blazor.BrowserStorage.Testing;

var local = new InMemoryLocalStorageService();
// or:
var local = new InMemoryLocalStorageService(new BrowserStorageOptions { KeyPrefix = "app_" });

await local.SetAsync("token", "abc123");
```

Cast to `IInMemoryStorage` for the test-time surface:

```csharp
var store = (IInMemoryStorage)local;
store.Seed("prefs", new UserPreferences { Theme = "Dark" });
Assert.Contains("app_prefs", store.Snapshot.Keys);
```

### Any test with `IServiceCollection`

Works with xUnit, MSTest, NUnit, integration tests, `WebApplicationFactory`, and anything else that uses `Microsoft.Extensions.DependencyInjection`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using D20Tek.Blazor.BrowserStorage.Testing;

var services = new ServiceCollection();
services.AddInMemoryBrowserStorage(o => o.KeyPrefix = "app_");
var sp = services.BuildServiceProvider();

var local = sp.GetRequiredService<ILocalStorageService>();
sp.GetInMemoryLocalStorage().Seed("prefs", new UserPreferences { Theme = "Dark" });
```

Register only one storage type with `AddInMemoryLocalStorage` or `AddInMemorySessionStorage`.

### bUnit component test

```csharp
using Bunit;
using D20Tek.Blazor.BrowserStorage;
using D20Tek.Blazor.BrowserStorage.Testing;

public class PreferencesPanelTests
{
	[Fact]
	public async Task Clicking_reset_clears_stored_preferences()
	{
		using var ctx = new BunitContext();
		ctx.AddBrowserStorage(o => o.KeyPrefix = "app_");

		ctx.GetLocalStorage().Seed("prefs", new UserPreferences { Theme = "Dark" });

		var cut = ctx.Render<PreferencesPanel>();
		cut.Find("button.reset").Click();

		Assert.False(ctx.GetLocalStorage().Snapshot.ContainsKey("app_prefs"));
	}
}
```

`AddBrowserStorage` registers both local and session storage. Use `AddLocalStorage` or `AddSessionStorage` to register only one.

## Arranging data with `Seed`

`Seed` is a **silent** arrange helper - it writes directly to the underlying store without raising the `Changed` event. The configured `KeyPrefix` is applied automatically.

```csharp
ctx.GetLocalStorage().Seed("token", "abc123");                 // typed
ctx.GetLocalStorage().Seed("session", "\"raw-json-string\"");  // raw JSON
```

Bulk overloads take any `IEnumerable<KeyValuePair<string, T>>` (typed) or `IEnumerable<KeyValuePair<string, string>>` (raw):

```csharp
ctx.GetLocalStorage().Seed(new Dictionary<string, int>
{
	["counter"] = 3,
	["retries"] = 0,
});
```

## Resetting between tests with `Clear`

`Clear` synchronously empties the store and - like `Seed` - does not raise `Changed`. Useful for shared fixtures or per-test teardown:

```csharp
ctx.GetLocalStorage().Clear();
```

## Inspecting the store

`Snapshot` returns a read-only view of the underlying dictionary. Keys appear in their prefixed form, matching what the real browser storage would see.

```csharp
IReadOnlyDictionary<string, string> snapshot = ctx.GetLocalStorage().Snapshot;
Assert.Contains("app_prefs", snapshot.Keys);
```

## Simulating storage unavailability

Real browser storage can be blocked (private mode, disabled site data, quota exhausted). Simulate that condition to test your fallback logic:

```csharp
var store = ctx.GetLocalStorage();
store.SimulateUnavailable();

var result = await ctx.Services
	.GetRequiredService<ILocalStorageService>()
	.SetAsync("k", 1);

Assert.False(result.IsSuccess);

store.RestoreAvailable();
```

## Simulating a cross-tab change

`RaiseExternalChange` fires the `Changed` event as if another browser tab had modified storage. It does not mutate the store.

```csharp
var local = ctx.Services.GetRequiredService<ILocalStorageService>();
using var _ = local.RecordChanges(out var events);

ctx.GetLocalStorage().RaiseExternalChange("theme", oldValue: "\"Light\"", newValue: "\"Dark\"");

Assert.Equal("theme", events[0].Key);
```

## Capturing `Changed` events with `RecordChanges`

`RecordChanges` is an extension on `IBrowserStorageService` that subscribes to the `Changed` event, returns an `IDisposable`, and exposes a thread-safe live list of captured events. It works with the in-memory services *and* the real `WebStorageService` - anywhere the event fires.

```csharp
var local = sp.GetRequiredService<ILocalStorageService>();
using (local.RecordChanges(out var events))
{
	await local.SetAsync("theme", "Dark");
	await local.SetAsync("fontSize", 14);

	Assert.Equal(2, events.Count);
	Assert.Equal("theme", events[0].Key);
}
// After dispose, later events are no longer captured.
```

## Swapping the real services in existing test hosts

For integration-test hosts that already register the production storage services (`WebApplicationFactory`, `TestServer`, custom hosts, etc.), use the `ReplaceWithInMemory*` helpers to remove the real registration and install the in-memory implementation in its place:

```csharp
using var factory = new WebApplicationFactory<Program>()
	.WithWebHostBuilder(builder =>
	{
		builder.ConfigureServices(services =>
		{
			services.ReplaceWithInMemoryBrowserStorage(o => o.KeyPrefix = "app_");
			// or, for one storage type only:
			// services.ReplaceWithInMemoryLocalStorage();
			// services.ReplaceWithInMemorySessionStorage();
		});
	});
```

Unlike `AddInMemoryBrowserStorage`, the `ReplaceWith*` methods explicitly remove any prior `ILocalStorageService` / `ISessionStorageService` registrations first, so they are safe to call after the production `AddBrowserStorage` has already run.

## Using with xUnit / MSTest / NUnit

The in-memory services and bUnit extensions are test-framework agnostic. Any framework that supports bUnit will work - the samples above use xUnit-style assertions but the same code runs unchanged under MSTest or NUnit.
