# Getting Started: Cookie Storage

This guide covers `ICookieStorageService`, which provides typed, async access to browser cookies through `document.cookie`. It follows the same `IBrowserStorageService` contract as `ILocalStorageService` and `ISessionStorageService`, so the read/write/remove patterns described in the [Detailed Getting Started Guide](getting-started-detailed.md) apply here as well. This document focuses on what is unique to cookies: configuration of cookie attributes, expiration, and the limitations of `document.cookie`.

## Table of Contents

- [Install](#install)
- [Registration](#registration)
- [Reading and Writing Cookies](#reading-and-writing-cookies)
- [Cookie Attributes](#cookie-attributes)
  - [Expiration](#expiration)
  - [Path and Domain](#path-and-domain)
  - [SameSite and Secure](#samesite-and-secure)
- [Limitations](#limitations)
- [Testing](#testing)

## Install

```bash
dotnet add package D20Tek.Blazor.BrowserStorage
```

## Registration

Register `ICookieStorageService` with `AddCookieStorage`. The method accepts two optional configuration delegates: one for the shared `BrowserStorageOptions` (key prefix, JSON options) and one for `CookieStorageOptions` (cookie attributes), plus an optional `ServiceLifetime`.

```csharp
using Microsoft.Extensions.DependencyInjection;
using D20Tek.Blazor.BrowserStorage;

builder.Services.AddCookieStorage(
	configure: options => options.KeyPrefix = "myapp:",
	configureCookie: cookieOptions =>
	{
		cookieOptions.DefaultExpiration = TimeSpan.FromDays(7);
		cookieOptions.Path = "/";
		cookieOptions.SameSite = CookieSameSiteMode.Lax;
	});
```

Inject `ICookieStorageService` into your components and services like any other DI dependency:

```razor
@inject ICookieStorageService CookieStorage
```

## Reading and Writing Cookies

`ICookieStorageService` uses the same `GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, and `ClearAllAsync` methods as the other storage services, returning `StorageResult<T>` and `StorageResult`:

```csharp
// Write a value using the configured DefaultExpiration
await CookieStorage.SetAsync("consent-given", true);

// Read a value
var result = await CookieStorage.GetAsync<bool>("consent-given");
bool consentGiven = result.IsSuccess && result.Value;

// Remove a single cookie
await CookieStorage.RemoveAsync("consent-given");
```

`ContainsKeyAsync`, `LengthAsync`, `GetKeysAsync`, `IsAvailableAsync`, and the `Changed` event all behave as documented in the [Detailed Getting Started Guide](getting-started-detailed.md), operating over the set of script-accessible cookies for the current page.

## Cookie Attributes

`CookieStorageOptions`, configured via the `configureCookie` delegate of `AddCookieStorage`, controls the attributes applied to every cookie written by the service.

### Expiration

`DefaultExpiration` (default: 30 days) is the lifetime applied to cookies when no explicit expiration is given. Set it to `null` to write session cookies that are removed when the browser is closed:

```csharp
builder.Services.AddCookieStorage(configureCookie: options =>
{
	options.DefaultExpiration = null; // session cookie
});
```

### Path and Domain

`Path` (default: `"/"`) scopes cookies to a URL path. `Domain` (default: `null`) scopes cookies to a host; when `null`, cookies are host-only and not shared with subdomains:

```csharp
builder.Services.AddCookieStorage(configureCookie: options =>
{
	options.Path = "/app";
	options.Domain = "example.com";
});
```

### SameSite and Secure

`SameSite` (default: `CookieSameSiteMode.Lax`) controls cross-site request behavior and accepts `Lax`, `Strict`, or `None`. `Secure` (default: `false`) restricts the cookie to HTTPS connections and **must** be `true` when `SameSite` is `CookieSameSiteMode.None`:

```csharp
builder.Services.AddCookieStorage(configureCookie: options =>
{
	options.SameSite = CookieSameSiteMode.None;
	options.Secure = true; // required when SameSite is None
});
```

## Limitations

Cookie storage has constraints that do not apply to `localStorage` or `sessionStorage`:

- **Only script-accessible cookies are visible.** `document.cookie` cannot read or write `HttpOnly` cookies set by the server. `ICookieStorageService` can only manage cookies that your Blazor app itself has written, or other non-`HttpOnly` cookies readable from script.
- **`ClearAllAsync` is destructive and ignores `KeyPrefix`.** It removes every script-accessible cookie for the current origin, including cookies written by other libraries or scripts. To remove only cookies owned by this service, enumerate `GetKeysAsync` and call `RemoveAsync` for each key instead.
- **`Changed` has no native cross-tab notification.** Unlike `localStorage`, the browser does not raise a storage event for cookie changes, so `Changed` only fires for writes and removals performed through this service instance in the current tab.
- **Cookies have a small size budget.** Browsers typically limit cookies to around 4 KB each. Prefer `localStorage` or `sessionStorage` for larger payloads, and reserve cookies for small values that genuinely need to be cookies (e.g., values also read by the server).

## Testing

The `D20Tek.Blazor.BrowserStorage.Testing` package provides `InMemoryCookieStorageService` and corresponding `IServiceCollection` extensions so you can test code that depends on `ICookieStorageService` without a real browser. See the [Testing Guide](testing.md) and [Testing API Reference](testing-api-reference.md) for full details.

```csharp
using Microsoft.Extensions.DependencyInjection;
using D20Tek.Blazor.BrowserStorage.Testing;

var services = new ServiceCollection();
services.AddInMemoryCookieStorage(o => o.KeyPrefix = "app_");
var sp = services.BuildServiceProvider();

var cookies = sp.GetRequiredService<ICookieStorageService>();
sp.GetInMemoryCookieStorage().Seed("consent-given", true);
```

The in-memory fake mirrors key/value and `Changed`-event semantics but does not model cookie-specific attributes such as path, domain, SameSite, or expiration.

For bUnit component tests, use the `AddCookieStorage` / `GetCookieStorage` extensions on `BunitContext`:

```csharp
using Bunit;
using D20Tek.Blazor.BrowserStorage.Testing;

using var ctx = new BunitContext();
ctx.AddCookieStorage(o => o.KeyPrefix = "app_");
ctx.GetCookieStorage().Seed("consent-given", true);
```
