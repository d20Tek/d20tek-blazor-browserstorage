namespace D20Tek.Blazor.BrowserStorage.Internal;

/// <summary>
/// Cookie-backed implementation of <see cref="ICookieStorageService"/>. Mirrors the API shape of
/// <see cref="WebStorageService"/> but reads and writes values through <c>document.cookie</c> via
/// <see cref="CookieJsInterop"/>, honoring <see cref="CookieStorageOptions"/> for cookie attributes.
/// </summary>
internal sealed class CookieStorageService : ICookieStorageService
{
    private const string StorageName = "cookieStorage";
    private static readonly IReadOnlyList<string> EmptyKeys = [];

    private readonly BrowserStorageOptions _options;
    private readonly CookieStorageOptions _cookieOptions;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly CookieJsInterop _interop;

    public CookieStorageService(
        IJSRuntime jsRuntime,
        IOptions<BrowserStorageOptions> options,
        IOptions<CookieStorageOptions> cookieOptions)
    {
        _options = options.Value;
        _cookieOptions = cookieOptions.Value;
        _jsonOptions = _options.JsonOptions ?? new(JsonSerializerDefaults.Web);

        if (!_jsonOptions.IsReadOnly)
        {
            _jsonOptions.MakeReadOnly(populateMissingResolver: true);
        }

        _interop = new CookieJsInterop(jsRuntime);
    }

    /// <inheritdoc />
    public event EventHandler<StorageChangedEventArgs>? Changed;

    /// <inheritdoc />
    public ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken = default) =>
        _interop.IsAvailableAsync(cancellationToken);

    /// <inheritdoc />
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public async ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(cancellationToken)) return StorageResult<T>.Failure(StorageMessages.Unavailable(StorageName));

        var json = await _interop.GetCookieAsync(_options.PrefixKey(key), cancellationToken);
        if (json is null) return StorageResult<T>.Failure(StorageMessages.KeyNotFound(StorageName, key));

        try
        {
            return StorageResult<T>.Success(StorageSerializer.Deserialize<T>(json, _jsonOptions));
        }
        catch (Exception ex) when (ex is JsonException
                                      or FormatException
                                      or OverflowException
                                      or NotSupportedException
                                      or ArgumentException)
        {
            return StorageResult<T>.Failure(ex.Message);
        }
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(TrimmingMessages.RequiresUnreferencedCode)]
    [RequiresDynamicCode(TrimmingMessages.RequiresDynamicCode)]
    public async ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(cancellationToken)) return StorageResult.Failure(StorageMessages.Unavailable(StorageName));

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await _interop.GetCookieAsync(prefixedKey, cancellationToken);
        var json = StorageSerializer.Serialize(value, _jsonOptions);
        var maxAgeSeconds = _cookieOptions.DefaultExpiration is { } expiration
            ? (long)expiration.TotalSeconds
            : (long?)null;

        try
        {
            await _interop.SetCookieAsync(
                prefixedKey,
                json,
                maxAgeSeconds,
                _cookieOptions.Path,
                _cookieOptions.Domain,
                _cookieOptions.ToSameSiteToken(),
                _cookieOptions.Secure,
                cancellationToken);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            return StorageResult.Failure(StorageMessages.WriteFailed(StorageName, key, ex));
        }

        RaiseChanged(key, DeserializeRaw(oldJson), value);
        return StorageResult.Success();
    }

    /// <inheritdoc />
    public async ValueTask<StorageResult> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(cancellationToken)) return StorageResult.Failure(StorageMessages.Unavailable(StorageName));

        var prefixedKey = _options.PrefixKey(key);
        var oldJson = await _interop.GetCookieAsync(prefixedKey, cancellationToken);

        try
        {
            await RemoveCookieAsync(prefixedKey, cancellationToken);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            return StorageResult.Failure(StorageMessages.RemoveFailed(StorageName, key, ex));
        }

        RaiseChanged(key, DeserializeRaw(oldJson), null);
        return StorageResult.Success();
    }

    /// <inheritdoc />
    /// <remarks>
    /// Removes every cookie currently visible to <c>document.cookie</c> (that is, every
    /// non-HttpOnly cookie for the current origin), not only those written by this service. The
    /// configured <see cref="BrowserStorageOptions.KeyPrefix"/> is not honored here &#8212; enumerate
    /// <see cref="GetKeysAsync(CancellationToken)"/> and call <see cref="RemoveAsync(string, CancellationToken)"/>
    /// to scope the delete to this service.
    /// </remarks>
    public async ValueTask<StorageResult> ClearAllAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken)) return StorageResult.Failure(StorageMessages.Unavailable(StorageName));

        try
        {
            var keys = await _interop.GetAllCookieKeysAsync(cancellationToken);
            foreach (var key in keys)
            {
                await RemoveCookieAsync(key, cancellationToken);
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException)
        {
            return StorageResult.Failure(StorageMessages.ClearFailed(StorageName, ex));
        }

        return StorageResult.Success();
    }

    /// <inheritdoc />
    public async ValueTask<bool> ContainsKeyAsync(string key, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        if (!await IsAvailableAsync(cancellationToken)) return false;

        return await _interop.GetCookieAsync(_options.PrefixKey(key), cancellationToken) is not null;
    }

    /// <inheritdoc />
    public async ValueTask<int> LengthAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken)) return 0;

        var keys = await _interop.GetAllCookieKeysAsync(cancellationToken);
        return keys.Count;
    }

    /// <inheritdoc />
    public async ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsAvailableAsync(cancellationToken)) return EmptyKeys;

        var keys = await _interop.GetAllCookieKeysAsync(cancellationToken);
        var result = new List<string>(keys.Count);
        foreach (var key in keys)
        {
            result.Add(_options.StripPrefix(key));
        }

        return result;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => _interop.DisposeAsync();

    private ValueTask RemoveCookieAsync(string prefixedKey, CancellationToken cancellationToken) =>
        _interop.RemoveCookieAsync(prefixedKey, _cookieOptions.Path, _cookieOptions.Domain, cancellationToken);

    private void RaiseChanged(string key, object? oldValue, object? newValue) =>
        Changed?.Invoke(this, new StorageChangedEventArgs(key, oldValue, newValue));

    private static object? DeserializeRaw(string? json) => json;
}
