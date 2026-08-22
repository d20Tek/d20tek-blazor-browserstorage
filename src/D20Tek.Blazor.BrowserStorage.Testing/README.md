# D20Tek.Blazor.BrowserStorage.Testing

Testing companion for [D20Tek.Blazor.BrowserStorage](https://www.nuget.org/packages/D20Tek.Blazor.BrowserStorage). Provides:

- In-memory implementations of `ILocalStorageService` and `ISessionStorageService` that exercise the same serialization, key-prefixing, and `StorageResult` semantics as the real service.
- bUnit `TestContext` extensions that wire everything up with one line.
- A test-time `IInMemoryStorage` surface for seeding, snapshot inspection, availability simulation, and cross-tab event simulation.

## Install

```bash
dotnet add package D20Tek.Blazor.BrowserStorage.Testing
```

## Usage

```csharp
using Bunit;
using D20Tek.Blazor.BrowserStorage.Testing;

using var ctx = new BunitContext();
ctx.AddBrowserStorage(o => o.KeyPrefix = "app_");
ctx.GetLocalStorage().Seed("prefs", new UserPreferences { Theme = "Dark" });

var cut = ctx.Render<PreferencesPanel>();
cut.Find("button.reset").Click();

Assert.False(ctx.GetLocalStorage().Snapshot.ContainsKey("app_prefs"));
```

See the [testing guide](https://github.com/d20Tek/d20tek-blazor-browserstorage/blob/main/docs/testing.md) for more examples.
