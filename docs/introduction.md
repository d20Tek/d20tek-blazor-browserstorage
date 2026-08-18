# Introducing D20Tek.Blazor.BrowserStorage

## The problem: browser storage in Blazor shouldn't be this hard

If you've built anything meaningful with Blazor WebAssembly, you've needed to persist data in the browser. User preferences, session tokens, form drafts, shopping carts, game state — the list is long. The browser provides `localStorage` and `sessionStorage` for exactly this purpose, but accessing them from Blazor requires JavaScript interop. That means writing JS glue code, managing serialization by hand, and handling the inevitable edge cases around missing keys, null values, and type mismatches.

For years, the community's answer was Blazored.LocalStorage. It wrapped the interop details in a clean C# API and became a fixture in most Blazor WebAssembly projects. Then it was deprecated. The packages were removed from NuGet.org, the repository went into maintenance-only mode, and developers were left to find alternatives or write their own.

If you're one of those developers — whether you need to replace Blazored in an existing project or you're starting fresh and want a reliable browser storage solution — D20Tek.Blazor.BrowserStorage was built to fill that gap.

## What this library does

D20Tek.Blazor.BrowserStorage provides typed, asynchronous access to both `localStorage` and `sessionStorage` from Blazor WebAssembly and client-side interactive render modes. You register the services through standard .NET dependency injection, inject them into your components or services, and call strongly-typed methods to read, write, and manage stored data. No JavaScript, no manual JSON handling, no string-based type casting.

The library handles the full lifecycle of browser-stored data: writing values, reading them back with proper deserialization, checking for key existence, enumerating keys, removing individual entries, clearing storage entirely, and performing batch operations. It also raises events when storage changes, so your UI can react without polling or manual refresh logic.

## Where Blazored fell short

Blazored.LocalStorage served the community well, but it had design decisions that created friction in practice. The most notable: calling `GetItemAsync<T>` with a key that didn't exist in storage would throw an exception. This meant every read had to be wrapped in a try-catch block or preceded by a `ContainKeyAsync` check — adding boilerplate and making the code harder to reason about.

D20Tek.Blazor.BrowserStorage takes a different approach. The `GetAsync<T>` method returns a `StorageResult<T>` — a lightweight record struct with an `IsSuccess` flag and a `Value` property. If the key exists and deserializes correctly, `IsSuccess` is `true` and `Value` holds your data. If the key is missing, `IsSuccess` is `false` and `Value` contains the default for that type. No exceptions, no ceremony. You write a single conditional and move on.

Blazored also split localStorage and sessionStorage into separate NuGet packages with separate registrations. D20Tek.Blazor.BrowserStorage bundles both in one package. A single `AddBrowserStorage()` call registers both `ILocalStorageService` and `ISessionStorageService`, or you can register them individually if you prefer.

## Problems it solves

**Key collisions in multi-module applications.** When multiple features or independently developed modules share the same browser origin, key names can collide silently. The library supports configurable key prefixes — set a prefix like `"myapp_"` and every key is automatically namespaced without changing your application code.

**Boilerplate around missing keys.** As described above, the result-based read pattern eliminates try-catch blocks and double-call patterns. You check `IsSuccess`, provide a fallback, and continue.

**Batch operations without loops.** When you need to save multiple values at once (a score, a timestamp, and a player name at the end of a game round, for instance) or clean up several session keys on navigation, the bulk extension methods — `SetMultipleAsync` and `RemoveMultipleAsync` — handle the iteration internally.

**Reacting to storage changes.** If your app has multiple components that depend on the same stored value, you can subscribe to the `Changed` event on the storage service. When any value is written or removed, subscribers receive the key name, old value, and new value without coupling the components to each other.

**Rigid service lifetimes.** Different architectures have different needs. The library lets you register its services as Scoped (the default and correct choice for most Blazor WASM apps), Singleton, or Transient, depending on your application's DI structure.

**Serialization customization.** The default JSON serialization works for the majority of cases, but when you need specific naming policies, custom converters, or particular formatting rules, you can provide your own `JsonSerializerOptions` at registration time.

## What it doesn't try to do

This is a client-side library. It wraps the browser's own `localStorage` and `sessionStorage` APIs and inherits their characteristics: data is stored as strings, is not encrypted, has origin-scoped visibility, and has size limits (typically 5-10 MB depending on the browser). It is not a database, not a server-synced cache, and not appropriate for sensitive data like passwords or tokens that require server-side protection.

For server-side Blazor (Blazor Server), Microsoft provides `ProtectedLocalStorage` and `ProtectedSessionStorage` with encryption and anti-tamper protection. Those should be used instead when the rendering model is server-based.

## Getting started

The library targets .NET 9.0 and .NET 10.0. Installation is a single package reference:

```bash
dotnet add package D20Tek.Blazor.BrowserStorage
```

Register the services in your `Program.cs`:

```csharp
using D20Tek.Blazor.BrowserStorage;

builder.Services.AddBrowserStorage();
```

Inject into any component or service:

```csharp
[Inject]
private ILocalStorageService LocalStorage { get; set; } = default!;
```

Read and write values:

```csharp
await LocalStorage.SetAsync("username", "Alice");

var result = await LocalStorage.GetAsync<string>("username");
if (result.IsSuccess)
{
	// result.Value is "Alice"
}
```

That's the entire setup. From here you can explore the full API surface, configure key prefixes, hook into change events, or migrate an existing Blazored-based project using the migration guide.

## Links

- **NuGet package:** [D20Tek.Blazor.BrowserStorage on NuGet.org](https://www.nuget.org/packages/D20Tek.Blazor.BrowserStorage)
- **Getting Started guide:** [Detailed Getting Started](getting-started-detailed.md)
- **API Reference:** [Complete API Reference](api-reference.md)
- **Migration from Blazored:** [Migration Guide](blazored-migration.md)
- **Source and samples:** [GitHub repository](https://github.com/d20Tek/d20tek-blazor-browserstorage)
