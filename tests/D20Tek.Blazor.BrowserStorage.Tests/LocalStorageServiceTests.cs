namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class LocalStorageServiceTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService(string keyPrefix = "")
    {
        var options = Options.Create(new BrowserStorageOptions { KeyPrefix = keyPrefix });
        return new LocalStorageService(_jsRuntime, options);
    }

    // --- GetAsync ---

    [TestMethod]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "42";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("age", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenKeyMissing()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("missing", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(default, result.Value);
    }

    [TestMethod]
    public async Task GetAsync_DeserializesComplexType()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "{\"name\":\"test\",\"value\":10}";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<TestData>("data", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("test", result.Value!.Name);
        Assert.AreEqual(10, result.Value.Value);
    }

    [TestMethod]
    public async Task GetAsync_UsesKeyPrefix()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "hello";
        var service = CreateService("app_");

        // Act
        await service.GetAsync<string>("key", CancellationToken.None);

        // Assert
        Assert.AreEqual("app_key", _jsRuntime.Invocations[0].Args[0]);
    }

    // --- SetAsync ---

    [TestMethod]
    public async Task SetAsync_SerializesAndStoresValue()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService();

        // Act
        await service.SetAsync("name", "John", CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.setItem");
        Assert.AreEqual("name", Args[0]);
        Assert.AreEqual("John", Args[1]);
    }

    [TestMethod]
    public async Task SetAsync_SerializesComplexType()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService();

        // Act
        await service.SetAsync("data", new TestData { Name = "x", Value = 5 }, CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.setItem");
        Assert.AreEqual("{\"name\":\"x\",\"value\":5}", Args[1]);
    }

    [TestMethod]
    public async Task SetAsync_UsesKeyPrefix()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService("pre_");

        // Act
        await service.SetAsync("key", "val", CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.setItem");
        Assert.AreEqual("pre_key", Args[0]);
    }

    [TestMethod]
    public async Task SetAsync_RaisesChangedEvent()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService();
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += (_, e) => eventArgs = e;

        // Act
        await service.SetAsync("key", "newValue", CancellationToken.None);

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("key", eventArgs.Key);
        Assert.IsNull(eventArgs.OldValue);
        Assert.AreEqual("newValue", eventArgs.NewValue);
    }

    // --- RemoveAsync ---

    [TestMethod]
    public async Task RemoveAsync_InvokesRemoveItem()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "\"old\"";
        var service = CreateService();

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.removeItem");
        Assert.AreEqual("key", Args[0]);
    }

    [TestMethod]
    public async Task RemoveAsync_UsesKeyPrefix()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService("ns_");

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.removeItem");
        Assert.AreEqual("ns_key", Args[0]);
    }

    [TestMethod]
    public async Task RemoveAsync_RaisesChangedEvent()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "\"old\"";
        var service = CreateService();
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += (_, e) => eventArgs = e;

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("key", eventArgs.Key);
        Assert.IsNull(eventArgs.NewValue);
    }

    // --- ClearAsync ---

    [TestMethod]
    public async Task ClearAsync_InvokesClear()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.ClearAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual("localStorage.clear", _jsRuntime.Invocations[0].Identifier);
    }

    // --- ContainsKeyAsync ---

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsTrue_WhenKeyExists()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "value";
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
        _jsRuntime.Results["localStorage.getItem"] = null;
        var service = CreateService();

        // Act
        var result = await service.ContainsKeyAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    // --- LengthAsync ---

    [TestMethod]
    public async Task LengthAsync_ReturnsCount()
    {
        // Arrange
        _jsRuntime.Results["eval"] = 3;
        var service = CreateService();

        // Act
        var result = await service.LengthAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(3, result);
    }

    // --- GetKeysAsync ---

    [TestMethod]
    public async Task GetKeysAsync_ReturnsAllKeys()
    {
        // Arrange
        _jsRuntime.Results["eval"] = 2;
        _jsRuntime.Results["localStorage.key"] = "key1";
        var service = CreateService();

        // Act
        var keys = await service.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.HasCount(2, keys);
    }

    [TestMethod]
    public async Task GetKeysAsync_StripsPrefix()
    {
        // Arrange
        _jsRuntime.Results["eval"] = 1;
        _jsRuntime.Results["localStorage.key"] = "app_mykey";
        var service = CreateService("app_");

        // Act
        var keys = await service.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual("mykey", keys[0]);
    }

    private sealed class TestData
    {
        public string Name { get; set; } = string.Empty;

        public int Value { get; set; }
    }
}
