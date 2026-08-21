namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class WebStorageServiceAvailabilityTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService(string keyPrefix = "")
    {
        var options = Options.Create(new BrowserStorageOptions { KeyPrefix = keyPrefix });
        return new LocalStorageService(_jsRuntime, options);
    }

    private void MakeStorageUnavailable() => _jsRuntime.Results["eval:probe"] = false;

    // --- IsAvailableAsync ---

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsTrue_WhenProbeSucceeds()
    {
        // Arrange
        var service = CreateService();

        // Act
        var result = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenProbeReturnsFalse()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenProbeThrows()
    {
        // Arrange
        _jsRuntime.ExceptionForIdentifier["eval"] = new JSException("storage blocked");
        var service = CreateService();

        // Act
        var result = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_CachesResult_AcrossMultipleCalls()
    {
        // Arrange
        var service = CreateService();

        // Act
        _ = await service.IsAvailableAsync(CancellationToken.None);
        _ = await service.IsAvailableAsync(CancellationToken.None);
        _ = await service.IsAvailableAsync(CancellationToken.None);

        // Assert
        var probeCount = _jsRuntime.Invocations.Count([ExcludeFromCodeCoverage](i) =>
            i.Identifier == "eval" &&
            i.Args.Length > 0 &&
            i.Args[0] is string s &&
            s.Contains("__d20tek_probe__"));
        Assert.AreEqual(1, probeCount);
    }

    [TestMethod]
    public async Task IsAvailableAsync_RunsProbeOnce_UnderConcurrentCallers()
    {
        // Arrange
        var service = CreateService();

        // Act
        var callers = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(async () => await service.IsAvailableAsync(CancellationToken.None)))
            .ToArray();
        var results = await Task.WhenAll(callers);

        // Assert
        Assert.IsTrue(results.All(r => r));
        var probeCount = _jsRuntime.Invocations.Count([ExcludeFromCodeCoverage](i) =>
            i.Identifier == "eval" &&
            i.Args.Length > 0 &&
            i.Args[0] is string s &&
            s.Contains("__d20tek_probe__"));
        Assert.AreEqual(1, probeCount);
    }

    // --- GetAsync ---

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("any-key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(default, result.Value);
        Assert.DoesNotContain(i => i.Identifier == "localStorage.getItem", _jsRuntime.Invocations);
    }

    // --- SetAsync ---

    [TestMethod]
    public async Task SetAsync_IsNoOp_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.DoesNotContain(i => i.Identifier == "localStorage.setItem", _jsRuntime.Invocations);
    }

    [TestMethod]
    public async Task SetAsync_DoesNotRaiseChanged_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += [ExcludeFromCodeCoverage] (_, e) => eventArgs = e;

        // Act
        await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        Assert.IsNull(eventArgs);
    }

    // --- RemoveAsync ---

    [TestMethod]
    public async Task RemoveAsync_IsNoOp_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        await service.RemoveAsync("key", CancellationToken.None);

        // Assert
        Assert.DoesNotContain(i => i.Identifier == "localStorage.removeItem", _jsRuntime.Invocations);
    }

    // --- ClearAllAsync ---

    [TestMethod]
    public async Task ClearAllAsync_IsNoOp_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        await service.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.DoesNotContain(i => i.Identifier == "localStorage.clear", _jsRuntime.Invocations);
    }

    // --- ContainsKeyAsync ---

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsFalse_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.ContainsKeyAsync("key", CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
        Assert.DoesNotContain(i => i.Identifier == "localStorage.getItem", _jsRuntime.Invocations);
    }

    // --- LengthAsync ---

    [TestMethod]
    public async Task LengthAsync_ReturnsZero_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.LengthAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(0, result);
        // No eval for length should be issued when unavailable (only the probe eval, if any).
        var lengthEvalCount = _jsRuntime.Invocations.Count([ExcludeFromCodeCoverage](i) =>
            i.Identifier == "eval" &&
            i.Args.Length > 0 &&
            i.Args[0] is string s &&
            s == "localStorage.length");
        Assert.AreEqual(0, lengthEvalCount);
    }

    // --- GetKeysAsync ---

    [TestMethod]
    public async Task GetKeysAsync_ReturnsEmpty_WhenStorageUnavailable()
    {
        // Arrange
        MakeStorageUnavailable();
        var service = CreateService();

        // Act
        var result = await service.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.IsEmpty(result);
        Assert.DoesNotContain(i => i.Identifier == "localStorage.key", _jsRuntime.Invocations);
    }
}
