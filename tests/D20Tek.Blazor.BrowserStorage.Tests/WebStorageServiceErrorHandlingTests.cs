namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class WebStorageServiceErrorHandlingTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService() =>
        new(_jsRuntime, Options.Create(new BrowserStorageOptions()));

    private void MakeStorageUnavailable() => _jsRuntime.Results["eval:probe"] = false;

    // --- SetAsync ---

    [TestMethod]
    public async Task SetAsync_ReturnsFailureResult_WhenJsInteropThrows()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.setItem"] = new JSException("QuotaExceededError");
        var service = CreateService();

        // Act
        var result = await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("key", result.ErrorMessage);
        Assert.Contains("QuotaExceededError", result.ErrorMessage);
    }

    [TestMethod]
    public async Task SetAsync_DoesNotRaiseChanged_WhenJsInteropThrows()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.setItem"] = new JSException("failed");
        var service = CreateService();
        var raised = 0;
        service.Changed += [ExcludeFromCodeCoverage](_, _) => raised++;

        // Act
        var result = await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsFailureResult_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("localStorage", result.ErrorMessage);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsSuccessResult_OnHappyPath()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.ErrorMessage);
    }

    // --- RemoveAsync ---

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailureResult_WhenJsInteropThrows()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.removeItem"] = new JSException("interop failed");
        var service = CreateService();

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("key", result.ErrorMessage);
        Assert.Contains("interop failed", result.ErrorMessage);
    }

    [TestMethod]
    public async Task RemoveAsync_DoesNotRaiseChanged_WhenJsInteropThrows()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "\"old\"";
        _jsRuntime.ExceptionForIdentifier["localStorage.removeItem"] = new JSException("failed");
        var service = CreateService();
        var raised = 0;
        service.Changed += [ExcludeFromCodeCoverage](_, _) => raised++;

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailureResult_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsSuccessResult_OnHappyPath()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.ErrorMessage);
    }

    // --- ClearAllAsync ---

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailureResult_WhenJsInteropThrows()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.clear"] = new JSException("clear failed");
        var service = CreateService();

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("clear failed", result.ErrorMessage);
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailureResult_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsSuccessResult_OnHappyPath()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.ErrorMessage);
    }

    // --- GetAsync error message population ---

    [TestMethod]
    public async Task GetAsync_PopulatesErrorMessage_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.GetAsync<string>("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("localStorage", result.ErrorMessage);
    }

    [TestMethod]
    public async Task GetAsync_PopulatesErrorMessage_WhenJsonIsMalformed()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "{ not valid";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task GetAsync_PopulatesErrorMessage_WhenKeyMissing()
    {
        // Arrange - no result configured means null (key missing)
        var service = CreateService();

        // Act
        var result = await service.GetAsync<string>("missing", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.Contains("missing", result.ErrorMessage);
    }
}

[TestClass]
public class BulkExtensionsErrorHandlingTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService() =>
        new(_jsRuntime, Options.Create(new BrowserStorageOptions()));

    [TestMethod]
    public async Task SetMultipleAsync_ReturnsSuccess_WhenAllItemsSucceed()
    {
        // Arrange
        var service = CreateService();
        var items = new Dictionary<string, object> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = await service.SetMultipleAsync(items, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task SetMultipleAsync_FailsFast_OnFirstFailure()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.setItem"] = new JSException("quota");
        var service = CreateService();
        var items = new Dictionary<string, object> { ["a"] = 1, ["b"] = 2 };

        // Act
        var result = await service.SetMultipleAsync(items, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        // Only the first key should have been attempted.
        var setInvocations = _jsRuntime.Invocations.Count(i => i.Identifier == "localStorage.setItem");
        Assert.AreEqual(1, setInvocations);
    }

    [TestMethod]
    public async Task RemoveMultipleAsync_ReturnsSuccess_WhenAllKeysSucceed()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.RemoveMultipleAsync(["a", "b"], CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task RemoveMultipleAsync_FailsFast_OnFirstFailure()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["localStorage.removeItem"] = new JSException("boom");
        var service = CreateService();

        // Act
        var result = await service.RemoveMultipleAsync(["a", "b"], CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
        var removeInvocations = _jsRuntime.Invocations.Count(i => i.Identifier == "localStorage.removeItem");
        Assert.AreEqual(1, removeInvocations);
    }
}
