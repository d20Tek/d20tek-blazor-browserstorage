# Future Features - D20Tek.Blazor.BrowserStorage

This document tracks 15 themes for potential future work across the `D20Tek.Blazor.BrowserStorage` and `D20Tek.Blazor.BrowserStorage.Testing` packages. Each theme groups related feature ideas; individual items may graduate to separate issues or milestones over time.

---

## 1. IndexedDB Storage Provider

Add a first-class provider for IndexedDB to complement `localStorage` and `sessionStorage`.

- `IIndexedDbStorageService` with async CRUD, key ranges, and cursor-based enumeration.
- Object stores, indexes, and versioned schema migrations expressed in options.
- Bulk operations and transactions across multiple stores.
- Fallback to `localStorage` when IndexedDB is unavailable (private mode, quota errors).

## 2. Cache Storage and Service Worker Integration [low priority]

Expose the Cache API and light-touch service worker helpers for PWA scenarios.

- `ICacheStorageService` for adding, matching, and deleting cached responses.
- Helpers to prefetch and pin critical assets on first load.
- Interop that plays nicely with Blazor WebAssembly service workers.

## 3. Cookie Storage Service [DONE]

Provide a typed cookie facade that mirrors the existing storage API shape.

- `ICookieStorageService` with `SameSite`, `Secure`, `HttpOnly` (where applicable), path, domain, and expiration options.
- Integration with `BrowserStorageOptions` prefixing and serialization.
- Consistent `StorageResult<T>` return type across all providers.

## 4. Cross-Tab Synchronization and Broadcast

Extend the existing storage-event listener with richer cross-tab primitives.

- Wrap the `BroadcastChannel` API as `IBroadcastChannelService<T>`.
- Optional leader election helper (single active tab) built on Web Locks.
- Debounced and coalesced change notifications to avoid event storms.

## 5. Reactive Streams and Observable Keys

Offer reactive access to storage values for Blazor components.

- `IObservable<StorageChangedEventArgs<T>>` per key.
- `ObservableStorageValue<T>` component/service that raises `StateHasChanged` automatically.
- `IAsyncEnumerable<T>` change feeds for advanced consumers.

## 6. Encryption, Signing, and Data Protection [low priority]

Protect sensitive values at rest inside browser storage.

- Pluggable `IStorageValueProtector` with AES-GCM defaults via `SubtleCrypto`.
- Key derivation from a per-user secret or a server-issued key.
- Optional HMAC signing for tamper detection on non-encrypted values.
- Guidance and analyzers to steer users away from storing secrets unprotected.

## 7. Expiration, TTL, and Sliding Windows

Add first-class time-based lifetime management on top of `localStorage`.

- `SetAsync(key, value, TimeSpan ttl)` and absolute expiration overloads.
- Sliding expiration with per-access renewal.
- Background cleanup of expired entries with a configurable strategy.
- Integrated metadata envelope so expirations survive tab reloads.

## 8. Quota Management and Storage Health

Give apps visibility and control over storage usage.

- `IStorageQuotaService` wrapping `navigator.storage.estimate()`.
- Configurable quota thresholds with `QuotaExceeded` events.
- Eviction policies (LRU, LFU, priority buckets) for oversized caches.
- `RequestPersistentStorageAsync()` helper.

## 9. Schema Versioning and Migrations

Support evolving data shapes without breaking existing users.

- Versioned envelopes: `{ "v": 3, "data": ... }`.
- `IStorageMigration` pipeline executed on read.
- Registration via `AddBrowserStorage(o => o.AddMigration<V2ToV3>())`.
- Diagnostics for failed migrations with safe fallback behavior.

## 10. Source Generators for Strongly Typed Keys

Reduce stringly-typed access and improve AOT/trimming support.

- `[BrowserStorageKey("theme")]` on partial classes to generate typed accessors.
- Generated JSON metadata registration for `System.Text.Json` source generation.
- Compile-time diagnostics for duplicate keys and reserved prefixes.

## 11. Blazor Component Library [low priority]

Ship a small set of ready-to-use components for common patterns.

- `<PersistentState TValue="..." Key="..." />` component for two-way binding.
- `<StorageChangeListener />` component surfacing `OnChanged` callbacks.
- `<StorageQuotaIndicator />` for diagnostics dashboards.
- Fully compatible with Blazor Server, WebAssembly, and Auto render modes.

## 12. Diagnostics, Logging, and Telemetry

Make it easier to debug storage in production.

- Structured `ILogger` output with categories per provider and key.
- `System.Diagnostics.Metrics` counters (read/write counts, errors, bytes).
- `Activity`/`ActivitySource` tracing around interop calls.
- Optional in-browser debug overlay for development builds.

## 13. Server-Side Prerender and Interactive Auto Support

Harden behavior across Blazor render modes.

- Explicit no-op or buffered behavior during static SSR/prerender.
- Deferred flush of writes once interactive mode is established.
- Documentation and samples for `.NET 9`/`.NET 10` Auto render mode.
- Analyzer that warns when storage APIs are called during prerender.

## 14. Testing Package Enhancements

Grow `D20Tek.Blazor.BrowserStorage.Testing` into a full-featured harness.

- In-memory fakes for the new IndexedDB, Cache, and Cookie services.
- Fluent assertions (`Should().HaveKey("x").WithValue(...)`).
- Recording/playback of storage change events for snapshot tests.
- bUnit extensions to configure storage state per test case.
- Deterministic clock injection for TTL and expiration tests.

## 15. Documentation, Samples, and Tooling

Invest in developer experience and adoption.

- Expand samples to cover PWA, offline-first, and multi-tab scenarios.
- Recipe-style guides (auth token caching, form autosave, feature flags).
- Roslyn analyzers and code fixes for common misuse patterns.
- Benchmarks comparing providers, serializers, and encryption overhead.
- Localized documentation and an interactive playground site.
