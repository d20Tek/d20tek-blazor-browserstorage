# Changelog

All notable changes to **D20Tek.Blazor.BrowserStorage** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- **`StorageResult` (non-generic)** result type for storage mutations, with `IsSuccess`, `ErrorMessage`, and `Success` / `Failure(string)` factory members.
- **`ErrorMessage`** property on `StorageResult<T>`; populated on failure paths for `GetAsync<T>` (unavailable storage, corrupt/mismatched JSON, missing key).
- **`IsAvailableAsync`** on `IBrowserStorageService` for probing whether the underlying browser storage is usable (private mode, disabled site data, quota exhausted). The result is cached and concurrency-safe.
- Argument validation on public methods and DI extensions (null/empty keys, null enumerables, null `IServiceCollection`).
- Corrupt-value protection in `GetAsync<T>` — malformed JSON, type mismatch, and wrong-shape payloads now return a failure result instead of throwing.
- Configurable DI service lifetimes (`Scoped`, `Singleton`, `Transient`) on `AddBrowserStorage`, `AddLocalStorage`, and `AddSessionStorage`.
- Multi-targeting for `.NET 9.0` and `.NET 10.0`.
- Symbol package (`.snupkg`) generation and source embedding for consumer debugging.
- New documentation: `docs/api-reference.md`, `docs/getting-started-detailed.md`, `docs/blazored-migration.md`.
- Sample applications: `PreferenceDashboard` and `SampleQuiz`.

### Changed

- **BREAKING**: `IBrowserStorageService.SetAsync<T>` now returns `ValueTask<StorageResult>` instead of `ValueTask`.
- **BREAKING**: `IBrowserStorageService.RemoveAsync` now returns `ValueTask<StorageResult>` instead of `ValueTask`.
- **BREAKING**: `IBrowserStorageService.ClearAsync` has been renamed to `ClearAllAsync` and now returns `ValueTask<StorageResult>` instead of `ValueTask`. The rename clarifies that the method wipes the entire browser storage area for the current origin — including keys written by other libraries — and does not honor the configured `KeyPrefix`. To delete only keys owned by this service, enumerate `GetKeysAsync` and call `RemoveAsync` for each.
- **BREAKING**: `BrowserStorageServiceBulkExtensions.SetMultipleAsync` and `RemoveMultipleAsync` now return `ValueTask<StorageResult>` with fail-fast semantics — the first failing per-item result is returned and remaining items are not attempted.
- **BREAKING**: `StorageResult<T>.Success` property was renamed to `IsSuccess` (from the initial API).
- `SetAsync<T>` no longer throws on JS interop failures (quota exceeded, storage disabled). Failures are surfaced through `StorageResult.ErrorMessage` instead.
- When storage is unavailable, mutations return a failure result rather than silently no-op'ing.

### Fixed

- Concurrency-safe availability probe using `Lazy<Task<bool>>` — the probe now runs only once even under concurrent callers.

## [1.0.2] - Initial release

### Added

- `ILocalStorageService` and `ISessionStorageService` with typed, async CRUD (`GetAsync<T>`, `SetAsync<T>`, `RemoveAsync`, `ClearAllAsync`).
- `StorageResult<T>` for result-based reads instead of exceptions on missing keys.
- `ContainsKeyAsync`, `LengthAsync`, `GetKeysAsync` query methods.
- `Changed` event with `StorageChangedEventArgs`.
- Bulk extension methods `SetMultipleAsync` and `RemoveMultipleAsync`.
- `BrowserStorageOptions` with `KeyPrefix` and custom `JsonOptions`.
- DI registration extensions: `AddBrowserStorage`, `AddLocalStorage`, `AddSessionStorage`.
