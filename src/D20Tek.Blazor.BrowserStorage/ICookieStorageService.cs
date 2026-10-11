namespace D20Tek.Blazor.BrowserStorage;

/// <summary>
/// Provides typed, async access to browser cookies, mirroring the shape of
/// <see cref="ILocalStorageService"/> and <see cref="ISessionStorageService"/>. Cookies are
/// written and read via <c>document.cookie</c>, so only script-accessible (non-HttpOnly) cookies
/// can be managed through this service. Cookie attributes (path, domain, SameSite, Secure, and
/// default expiration) are configured via <see cref="CookieStorageOptions"/>.
/// </summary>
/// <remarks>
/// Unlike <see cref="ILocalStorageService"/> and <see cref="ISessionStorageService"/>, browsers do
/// not raise a native change event for cookie writes from other tabs, so the inherited
/// <see cref="IBrowserStorageService.Changed"/> event only fires for writes and removals performed
/// through this service instance.
/// </remarks>
public interface ICookieStorageService : IBrowserStorageService;
