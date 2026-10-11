namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class CookieJsInteropTests
{
    private readonly FakeJSRuntime _jsRuntime = new();
    private readonly FakeJSObjectReference _module = new();

    private CookieJsInterop CreateInterop()
    {
        _jsRuntime.ModuleReference = _module;
        return new CookieJsInterop(_jsRuntime);
    }

    [TestMethod]
    public async Task DisposeAsync_DisposesModule_WhenModuleWasImported()
    {
        // Arrange
        var interop = CreateInterop();
        await interop.GetCookieAsync("key", CancellationToken.None);

        // Act
        await interop.DisposeAsync();

        // Assert
        Assert.IsTrue(_module.Disposed);
    }

    [TestMethod]
    public async Task DisposeAsync_DoesNotThrow_WhenModuleNeverImported()
    {
        // Arrange
        var interop = CreateInterop();

        // Act
        await interop.DisposeAsync();

        // Assert - no exception, module was never imported so nothing to dispose
        Assert.IsFalse(_module.Disposed);
    }

    [TestMethod]
    public async Task DisposeAsync_SwallowsJsDisconnectedException_WhenCircuitAlreadyTornDown()
    {
        // Arrange
        var interop = CreateInterop();
        await interop.GetCookieAsync("key", CancellationToken.None);
        _module.ExceptionOnDispose = new JSDisconnectedException("circuit gone");

        // Act
        await interop.DisposeAsync();

        // Assert - no exception propagated, even though the module's DisposeAsync threw
        Assert.IsTrue(_module.Disposed);
    }

    // --- IsAvailableAsync ---

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsTrue_WhenModuleReportsAvailable()
    {
        // Arrange
        _module.Results["isCookieAvailable"] = true;
        var interop = CreateInterop();

        // Act
        var result = await interop.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenJsDisconnectedExceptionThrown()
    {
        // Arrange
        _jsRuntime.ModuleReference = null;
        _jsRuntime.ExceptionForIdentifier["import"] = new JSDisconnectedException("circuit gone");
        var interop = new CookieJsInterop(_jsRuntime);

        // Act
        var result = await interop.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenInvalidOperationExceptionThrown()
    {
        // Arrange
        _jsRuntime.ModuleReference = null;
        _jsRuntime.ExceptionForIdentifier["import"] = new InvalidOperationException("prerendering");
        var interop = new CookieJsInterop(_jsRuntime);

        // Act
        var result = await interop.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_WhenUnexpectedExceptionThrown()
    {
        // Arrange
        _jsRuntime.ModuleReference = null;
        _jsRuntime.ExceptionForIdentifier["import"] = new InvalidCastException("unexpected");
        var interop = new CookieJsInterop(_jsRuntime);

        // Act
        var result = await interop.IsAvailableAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result);
    }
}
