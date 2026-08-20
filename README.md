[![Build Status](https://github.com/d20Tek/d20tek-blazor-browserstorage/actions/workflows/ci-build.yml/badge.svg)](https://github.com/d20Tek/d20tek-blazor-browserstorage/actions)
[![NuGet](https://img.shields.io/nuget/v/D20Tek.Blazor.BrowserStorage)](https://www.nuget.org/packages/D20Tek.Blazor.BrowserStorage)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

# D20Tek.Blazor.BrowserStorage

A modern, lightweight .NET library that provides typed, asynchronous access to the browser's `localStorage` and `sessionStorage` APIs for Blazor WebAssembly and client-side render modes. Built using JavaScript interop, this library eliminates the need for manual serialization, raw JS calls, or string-based key management. It offers a clean, strongly-typed C# API with full dependency injection support.

This package was inspired by Blazored.LocalStorage/SessionStorage. Once I realized that it was deprecated and removed from NuGet.org, I needed my own implementation to use across my Blazor apps. While building on the basic functionality, D20Tek.Blazor.BrowserStorage provides additional features and a more modern, flexible API. The most significant difference is that `GetAsync<T>` returns a `StorageResult<T>` instead of throwing an exception when a key is not found, allowing for more graceful handling of missing keys.

## Table of Contents

- [Features](#features)
- [Supported Platforms](#supported-platforms)
- [Installation](#installation)
- [Quick Start Guide](#quick-start-guide)
- [Usage](docs/getting-started-detailed.md#usage)
  - [Reading Values](docs/getting-started-detailed.md#reading-values)
  - [Writing Values](docs/getting-started-detailed.md#writing-values)
  - [Removing and Clearing Data](docs/getting-started-detailed.md#removing-and-clearing-data)
  - [Checking for Keys](docs/getting-started-detailed.md#checking-for-keys)
  - [Enumerating Storage](docs/getting-started-detailed.md#enumerating-storage)
  - [Bulk Operations](docs/getting-started-detailed.md#bulk-operations)
  - [Change Notifications](docs/getting-started-detailed.md#change-notifications)
- [Configuration](docs/getting-started-detailed.md#configuration)
  - [Key Prefixing](docs/getting-started-detailed.md#key-prefixing)
  - [Custom JSON Serialization](docs/getting-started-detailed.md#custom-json-serialization)
  - [Service Lifetime](docs/getting-started-detailed.md#service-lifetime)
- [API Reference](docs/api-reference.md)
- [Sample Applications](#sample-applications)
- [Migration from Blazored.LocalStorage](docs/blazored-migration.md)
- [License](#license)

## Why This Library Exists
Blazor does not provide a built‑in way to access localStorage or sessionStorage from .NET. The only way to use browser storage is to call JavaScript manually through IJSRuntime, which leads to several problems:
- You must write JS interop boilerplate for every read/write.
- Storage access becomes stringly‑typed, error‑prone, and repetitive.
- You have to handle JSON serialization yourself.
- There’s no clean way to expose storage as a typed .NET service.
- Previously used Blazored.LocalStorage package for these purposes, but that has been deprecated and no longer available.

For a framework that encourages strong typing, DI, and clean architecture, browser storage ends up feeling like a low‑level workaround. And this library exists to fix that.

D20Tek.Blazor.BrowserStorage provides:
- A fully typed storage API
- A clean async interface
- Zero‑boilerplate JSON handling
- A simple, DI‑friendly .NET service
- Support for both localStorage and sessionStorage
- A modern API designed for Blazor WebAssembly and Blazor SSR

It gives Blazor developers a first‑class, standard way to use browser storage, without having to touch JavaScript.

## Features

- **Typed, async API**: Read and write any serializable .NET type with generic `GetAsync<T>` and `SetAsync<T>` methods. No manual JSON handling required.
- **Result-based reads**: `GetAsync<T>` returns a `StorageResult<T>` with an `IsSuccess` flag, eliminating exceptions on missing keys and enabling safe fallback patterns.
- **localStorage and sessionStorage**: Full support for both browser storage mechanisms through `ILocalStorageService` and `ISessionStorageService`.
- **Bulk operations**: `SetMultipleAsync` and `RemoveMultipleAsync` extension methods for batch read/write scenarios.
- **Key prefix namespacing**: Configure a prefix string (for example, `"myapp_"`) that is automatically prepended to all keys, preventing collisions between multiple applications or modules sharing the same origin.
- **Change notifications**: Subscribe to the `Changed` event on either service to receive `StorageChangedEventArgs` whenever a value is added, modified, or removed.
- **Configurable JSON serialization**: Provide custom `JsonSerializerOptions` for scenarios that require specific naming policies, converters, or formatting.
- **Flexible service lifetimes**: Register services as Scoped (default), Singleton, or Transient to match your application's architecture.
- **Lightweight and focused**: No external dependencies beyond the standard Microsoft.JSInterop and Microsoft.Extensions packages.

## Supported Platforms

| Target Framework | Status |
|---|---|
| .NET 9.0 | Supported |
| .NET 10.0 | Supported |

This library is designed for Blazor WebAssembly and Blazor client-side interactive render modes. It is not intended for server-side Blazor (Blazor Server), where Microsoft's built-in `ProtectedLocalStorage` and `ProtectedSessionStorage` should be used instead.

## Installation

Install the package via the .NET CLI:

```bash
dotnet add package D20Tek.Blazor.BrowserStorage
```

Or via the NuGet Package Manager in Visual Studio:

```
Install-Package D20Tek.Blazor.BrowserStorage
```

## Quick Start Guide

### 1. Register services in Program.cs

Register both localStorage and sessionStorage services with a single call:

```csharp
using D20Tek.Blazor.BrowserStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBrowserStorage();

await builder.Build().RunAsync();
```

If you only need one of the two storage types, you can register them individually:

```csharp
builder.Services.AddLocalStorage();
// or
builder.Services.AddSessionStorage();
```

### 2. Inject the service into a component

```csharp
@inject ILocalStorageService LocalStorage
@inject ISessionStorageService SessionStorage
```

Or in a code-behind file:

```csharp
[Inject]
private ILocalStorageService LocalStorage { get; set; } = default!;
```

### 3. Read and write values

```csharp
// Write a value
await LocalStorage.SetAsync("username", "Alice");

// Read a value
var result = await LocalStorage.GetAsync<string>("username");
if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // "Alice"
}
```

## Usage and Configuration
For detailed usage instructions covering all storage operations (reading, writing, removing, checking keys, enumerating, bulk operations, and change notifications) as well as configuration options (key prefixing, custom JSON serialization, and service lifetimes), see the [Detailed Getting Started Guide](docs/getting-started-detailed.md).

## API Reference
For a complete reference of all public interfaces, methods, events, extension methods, and types, see the [API Reference](docs/api-reference.md).

## Sample Applications
The repository includes two sample Blazor WebAssembly applications that demonstrate the library's features in realistic scenarios:

### [PreferenceDashboard](samples/PreferenceDashboard)
A settings and preferences dashboard that uses `localStorage` to persist visual preferences (theme, accent color, and font family) across browser sessions, and `sessionStorage` to track dismissal of a "What's New" banner within the current tab.

**Features demonstrated:** `GetAsync<T>`, `SetAsync<T>`, `ContainsKeyAsync`, `ClearAsync`, key prefix namespacing, and the `Changed` event.

### [SampleQuiz](samples/SampleQuiz)
An interactive tech trivia game with 100 questions across .NET, Azure, and Windows categories. The application uses `localStorage` for persistent data such as player profiles, high scores, and game statistics, and `sessionStorage` for current quiz state with recovery on page refresh.

**Features demonstrated:** Complex object serialization, `StorageResult<T>` for first-time player detection, `SetMultipleAsync` for batch result saving, `RemoveMultipleAsync` for session cleanup, `GetKeysAsync` and `LengthAsync` for storage statistics, and `ClearAsync` for data reset.

## Migration from Blazored.LocalStorage
If you are migrating from the Blazored.LocalStorage and Blazored.SessionStorage packages, see the [Migration Guide](docs/blazored-migration.md) for a detailed comparison of methods, return types, and registration patterns.

## Used by
Used in my Blazor projects and internal apps.

## License
This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
