namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class SessionStorageServiceTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    [TestMethod]
    public async Task SetAsync_UsesSessionStorageName()
    {
        // Arrange
        _jsRuntime.Results["sessionStorage.getItem"] = null;
        var options = Options.Create(new BrowserStorageOptions());
        var service = new SessionStorageService(_jsRuntime, options);

        // Act
        await service.SetAsync("key", "value", CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "sessionStorage.setItem");
        Assert.AreEqual("key", Args[0]);
        Assert.AreEqual("value", Args[1]);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsValue_WhenKeyExists()
    {
        // Arrange
        _jsRuntime.Results["sessionStorage.getItem"] = "42";
        var options = Options.Create(new BrowserStorageOptions());
        var service = new SessionStorageService(_jsRuntime, options);

        // Act
        var result = await service.GetAsync<int>("count", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(42, result.Value);
    }
}
