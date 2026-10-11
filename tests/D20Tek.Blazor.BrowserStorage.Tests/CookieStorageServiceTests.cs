namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class CookieStorageServiceTests
{
    private readonly FakeJSRuntime _jsRuntime = new();
    private readonly FakeJSObjectReference _module = new();

    private CookieStorageService CreateService(string keyPrefix = "", Action<CookieStorageOptions>? configureCookie = null)
    {
        _jsRuntime.ModuleReference = _module;
        _module.Results["isCookieAvailable"] = true;

        var options = Options.Create(new BrowserStorageOptions { KeyPrefix = keyPrefix });
        var cookieOptions = new CookieStorageOptions();
        configureCookie?.Invoke(cookieOptions);

        return new CookieStorageService(_jsRuntime, options, Options.Create(cookieOptions));
    }

    // --- GetAsync ---

    [TestMethod]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        _module.Results["getCookie"] = "42";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("age", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenKeyMissing()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("missing", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(default, result.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.GetAsync<string>("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task GetAsync_UsesKeyPrefix()
    {
        // Arrange
        _module.Results["getCookie"] = "\"hello\"";
        var service = CreateService("app_");

        // Act
        await service.GetAsync<string>("key", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "getCookie");
        Assert.AreEqual("app_key", args[0]);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenValueCorrupt()
    {
        // Arrange
        _module.Results["getCookie"] = "{not-json";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<TestData>("data", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    // --- SetAsync ---

    [TestMethod]
    public async Task SetAsync_SerializesAndStoresValue()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService();

        // Act
        await service.SetAsync("name", "John", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "setCookie");
        Assert.AreEqual("name", args[0]);
        Assert.AreEqual("John", args[1]);
    }

    [TestMethod]
    public async Task SetAsync_UsesConfiguredCookieAttributes()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService(configureCookie: o =>
        {
            o.Path = "/app";
            o.Domain = "example.com";
            o.SameSite = CookieSameSiteMode.Strict;
            o.Secure = true;
            o.DefaultExpiration = TimeSpan.FromSeconds(120);
        });

        // Act
        await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "setCookie");
        Assert.AreEqual(120L, args[2]);
        Assert.AreEqual("/app", args[3]);
        Assert.AreEqual("example.com", args[4]);
        Assert.AreEqual("Strict", args[5]);
        Assert.IsTrue((bool?)args[6]);
    }

    [TestMethod]
    public async Task SetAsync_PassesNullMaxAge_WhenNoDefaultExpiration()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService(configureCookie: o => o.DefaultExpiration = null);

        // Act
        await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "setCookie");
        Assert.IsNull(args[2]);
    }

    [TestMethod]
    public async Task SetAsync_UsesKeyPrefix()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService("pre_");

        // Act
        await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "setCookie");
        Assert.AreEqual("pre_key", args[0]);
    }

    [TestMethod]
    public async Task SetAsync_RaisesChangedEvent()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService();
        using var _ = service.RecordChanges(out var events);

        // Act
        await service.SetAsync("key", "newValue", CancellationToken.None);

        // Assert
        Assert.HasCount(1, events);
        Assert.AreEqual("key", events[0].Key);
        Assert.IsNull(events[0].OldValue);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsFailure_WhenSetCookieThrowsJsException()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        _module.ExceptionForIdentifier["setCookie"] = new JSException("boom");
        var service = CreateService();

        // Act
        var result = await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsFailure_WhenSetCookieThrowsJsDisconnectedException()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        _module.ExceptionForIdentifier["setCookie"] = new JSDisconnectedException("circuit gone");
        var service = CreateService();

        // Act
        var result = await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    // --- RemoveAsync ---

    [TestMethod]
    public async Task RemoveAsync_InvokesRemoveCookie()
    {
        // Arrange
        _module.Results["getCookie"] = "\"old\"";
        var service = CreateService();

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        var (_, args) = _module.Invocations.First(i => i.Identifier == "removeCookie");
        Assert.AreEqual("key", args[0]);
    }

    [TestMethod]
    public async Task RemoveAsync_RaisesChangedEvent()
    {
        // Arrange
        _module.Results["getCookie"] = "\"old\"";
        var service = CreateService();
        using var _ = service.RecordChanges(out var events);

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.HasCount(1, events);
        Assert.IsNull(events[0].NewValue);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailure_WhenRemoveCookieThrowsJsException()
    {
        // Arrange
        _module.Results["getCookie"] = "\"old\"";
        _module.ExceptionForIdentifier["removeCookie"] = new JSException("boom");
        var service = CreateService();

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailure_WhenRemoveCookieThrowsJsDisconnectedException()
    {
        // Arrange
        _module.Results["getCookie"] = "\"old\"";
        _module.ExceptionForIdentifier["removeCookie"] = new JSDisconnectedException("circuit gone");
        var service = CreateService();

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    // --- ClearAllAsync ---

    [TestMethod]
    public async Task ClearAllAsync_RemovesAllCookieKeys()
    {
        // Arrange
        _module.Results["getAllCookieKeys"] = new List<string> { "a", "b" };
        var service = CreateService();

        // Act
        await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(2, _module.Invocations.Count(i => i.Identifier == "removeCookie"));
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailure_WhenGetAllCookieKeysThrowsJsException()
    {
        // Arrange
        _module.ExceptionForIdentifier["getAllCookieKeys"] = new JSException("boom");
        var service = CreateService();

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailure_WhenRemoveCookieThrowsJsDisconnectedException()
    {
        // Arrange
        _module.Results["getAllCookieKeys"] = new List<string> { "a" };
        _module.ExceptionForIdentifier["removeCookie"] = new JSDisconnectedException("circuit gone");
        var service = CreateService();

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    // --- ContainsKeyAsync ---

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsTrue_WhenKeyExists()
    {
        // Arrange
        _module.Results["getCookie"] = "value";
        var service = CreateService();

        // Act
        var result = await service.ContainsKeyAsync("key", CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsFalse_WhenKeyMissing()
    {
        // Arrange
        _module.Results["getCookie"] = null;
        var service = CreateService();

        // Act
        var result = await service.ContainsKeyAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsFalse_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.ContainsKeyAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    // --- LengthAsync ---

    [TestMethod]
    public async Task LengthAsync_ReturnsCookieCount()
    {
        // Arrange
        _module.Results["getAllCookieKeys"] = new List<string> { "a", "b", "c" };
        var service = CreateService();

        // Act
        var result = await service.LengthAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public async Task LengthAsync_ReturnsZero_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.LengthAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(0, result);
    }

    // --- GetKeysAsync ---

    [TestMethod]
    public async Task GetKeysAsync_ReturnsAllKeys_WithoutPrefix()
    {
        // Arrange
        _module.Results["getAllCookieKeys"] = new List<string> { "app_a", "app_b" };
        var service = CreateService("app_");

        // Act
        var result = await service.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.AreSequenceEqual(["a", "b"], result.ToList(), SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public async Task GetKeysAsync_ReturnsEmpty_WhenUnavailable()
    {
        // Arrange
        var service = CreateService();
        _module.Results["isCookieAvailable"] = false;

        // Act
        var result = await service.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.IsEmpty(result);
    }

    // --- IsAvailableAsync ---

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsTrue_ByDefault()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenModuleThrows()
    {
        // Arrange
        _jsRuntime.ModuleReference = null;
        _jsRuntime.ExceptionForIdentifier["import"] = new JSException("boom");
        var options = Options.Create(new BrowserStorageOptions());
        var service = new CookieStorageService(_jsRuntime, options, Options.Create(new CookieStorageOptions()));

        // Act
        var result = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    // --- DisposeAsync ---

    [TestMethod]
    public async Task DisposeAsync_DisposesModule()
    {
        // Arrange
        var service = CreateService();
        await service.IsAvailableAsync(CancellationToken.None);

        // Act
        await service.DisposeAsync();

        // Assert
        Assert.IsTrue(_module.Disposed);
    }

    [TestMethod]
    public async Task DisposeAsync_DoesNotThrow_WhenModuleNeverImported()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.DisposeAsync();

        // Assert - no exception
    }

    private sealed class TestData
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
