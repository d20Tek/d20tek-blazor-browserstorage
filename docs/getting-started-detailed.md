# Detailed Getting Started Guide

This document provides detailed usage instructions and configuration options for D20Tek.Blazor.BrowserStorage. For a quick-start guide, see the [Quick Start](../README.md#quick-start-guide) section in the main README.

## Table of Contents

- [Usage](#usage)
  - [Reading Values](#reading-values)
  - [Writing Values](#writing-values)
  - [Removing and Clearing Data](#removing-and-clearing-data)
  - [Checking for Keys](#checking-for-keys)
  - [Enumerating Storage](#enumerating-storage)
  - [Bulk Operations](#bulk-operations)
  - [Change Notifications](#change-notifications)
- [Configuration](#configuration)
  - [Key Prefixing](#key-prefixing)
  - [Custom JSON Serialization](#custom-json-serialization)
  - [Service Lifetime](#service-lifetime)

## Usage

### Reading Values

The `GetAsync<T>` method returns a `StorageResult<T>` rather than throwing an exception when a key is not found. This design allows callers to handle missing keys gracefully without try/catch blocks.

```csharp
// Simple types
var result = await LocalStorage.GetAsync<int>("visit-count");
int visits = result.IsSuccess ? result.Value : 0;

// Complex objects
var profileResult = await LocalStorage.GetAsync<UserProfile>("user-profile");
if (profileResult.IsSuccess && profileResult.Value is not null)
{
	var profile = profileResult.Value;
}
```

The `StorageResult<T>` record struct contains two properties:

| Property | Type | Description |
|---|---|---|
| `IsSuccess` | `bool` | Indicates whether the key was found and the value was deserialized successfully. |
| `Value` | `T?` | The deserialized value, or the default value of `T` if the key was not found. |

### Writing Values

The `SetAsync<T>` method serializes the provided value to JSON and stores it under the specified key. Any serializable .NET type can be stored, including primitive types, collections, and complex objects.

```csharp
// Primitive types
await LocalStorage.SetAsync("theme", "dark");
await LocalStorage.SetAsync("font-size", 16);
await LocalStorage.SetAsync("notifications-enabled", true);

// Complex objects
var profile = new UserProfile
{
	Name = "Alice",
	CreatedDate = DateTimeOffset.UtcNow
};
await LocalStorage.SetAsync("user-profile", profile);

// Collections
var scores = new List<ScoreEntry> { new() { Score = 95, Date = DateTimeOffset.UtcNow } };
await LocalStorage.SetAsync("high-scores", scores);
```

### Removing and Clearing Data

Remove a single key or clear all keys from storage:

```csharp
// Remove a specific key
await LocalStorage.RemoveAsync("username");

// Clear all keys from storage
await LocalStorage.ClearAsync();
```

### Checking for Keys

Use `ContainsKeyAsync` to check whether a key exists without reading its value:

```csharp
bool exists = await LocalStorage.ContainsKeyAsync("user-profile");
if (!exists)
{
	// First-time visitor: create default profile
}
```

### Enumerating Storage

Retrieve the number of stored keys or list all key names:

```csharp
// Get the total number of keys
int count = await LocalStorage.LengthAsync();

// Get all key names
IReadOnlyList<string> keys = await LocalStorage.GetKeysAsync();
foreach (var key in keys)
{
	Console.WriteLine(key);
}
```

### Bulk Operations

The `SetMultipleAsync` and `RemoveMultipleAsync` extension methods allow batch operations in a single logical call. These methods iterate over the provided items and perform individual storage operations for each entry.

```csharp
// Write multiple values at once
var items = new List<KeyValuePair<string, object>>
{
	new("high-scores", updatedScores),
	new("games-played", gamesPlayed),
	new("last-played", DateTimeOffset.UtcNow)
};
await LocalStorage.SetMultipleAsync(items);

// Remove multiple keys at once
await SessionStorage.RemoveMultipleAsync(["quiz-state", "current-streak", "timer"]);
```

### Change Notifications

Both `ILocalStorageService` and `ISessionStorageService` expose a `Changed` event that fires whenever a value is written or removed through the service. This is useful for updating UI elements in response to storage changes.

```csharp
LocalStorage.Changed += (sender, args) =>
{
	Console.WriteLine($"Key '{args.Key}' changed from '{args.OldValue}' to '{args.NewValue}'");
	StateHasChanged();
};
```

The `StorageChangedEventArgs` class provides the following properties:

| Property | Type | Description |
|---|---|---|
| `Key` | `string` | The storage key that was modified. |
| `OldValue` | `object?` | The previous value, or null if the key is new. |
| `NewValue` | `object?` | The new value, or null if the key was removed. |

## Configuration

### Key Prefixing

Configure a key prefix to namespace all storage keys and prevent collisions with other applications or libraries sharing the same browser origin:

```csharp
builder.Services.AddBrowserStorage(options =>
{
	options.KeyPrefix = "myapp:";
});
```

With this configuration, a call to `SetAsync("theme", "dark")` will store the value under the key `"myapp:theme"` in the browser. The prefix is applied transparently and does not affect the keys used in your application code.

### Custom JSON Serialization

Provide custom `JsonSerializerOptions` to control how values are serialized and deserialized:

```csharp
builder.Services.AddBrowserStorage(options =>
{
	options.JsonOptions = new JsonSerializerOptions
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
	};
});
```

When no custom options are provided, the library uses `JsonSerializerDefaults.Web`, which applies camelCase property naming and case-insensitive deserialization by default.

### Service Lifetime

By default, services are registered with a `Scoped` lifetime, which is the standard for Blazor WebAssembly. You can change the lifetime to `Singleton` or `Transient` if your application requires it:

```csharp
using Microsoft.Extensions.DependencyInjection;

// Register as Singleton
builder.Services.AddBrowserStorage(lifetime: ServiceLifetime.Singleton);

// Register with options and a custom lifetime
builder.Services.AddLocalStorage(
	options => options.KeyPrefix = "app:",
	lifetime: ServiceLifetime.Transient
);
```
