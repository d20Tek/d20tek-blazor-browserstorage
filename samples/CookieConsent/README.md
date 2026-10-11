# CookieConsent Sample

A Blazor WebAssembly sample demonstrating `ICookieStorageService` from `D20Tek.Blazor.BrowserStorage`, through a realistic cookie-consent banner scenario.

## What it demonstrates

- Registering `ICookieStorageService` with `AddCookieStorage`, configuring `CookieStorageOptions` (`DefaultExpiration`, `Path`, `SameSite`) in `Program.cs`.
- Reading and writing a typed value (`ConsentPreferences`) through `GetAsync<T>` / `SetAsync<T>`, the same pattern used by `ILocalStorageService` and `ISessionStorageService`.
- Using `RemoveAsync` to clear a cookie and re-trigger the consent banner.
- Why a cookie is a better fit than `localStorage` for this scenario: the decision could also be read by a server-rendered page on the same domain, and it is configured to persist for a full year across browser restarts.
- A `Changed`-driven service (`ConsentBannerService`) that keeps the banner and the privacy page in sync without manual polling.

## Pages

- **Home** (`/`) - explains the scenario and shows the live, currently stored consent decision.
- **Privacy & Cookies** (`/privacy`) - lets visitors review, change, or reset their consent decision at any time.
- A persistent **consent banner** appears at the bottom of every page until a decision has been made, with "Accept All", "Reject Non-Essential", and "Customize" actions.

## Run it

```bash
dotnet run --project samples/CookieConsent/CookieConsent.csproj
```

Then open the app in a browser. Accept or reject cookies from the banner, inspect the live state on the Home page, and use the Privacy page to change or reset your decision.
