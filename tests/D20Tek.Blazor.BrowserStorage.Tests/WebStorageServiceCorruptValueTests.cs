namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class WebStorageServiceCorruptValueTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService() =>
        new(_jsRuntime, Options.Create(new BrowserStorageOptions()));

    [ExcludeFromCodeCoverage]
    private sealed record Person(string Name, int Age);

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenJsonIsMalformed()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "{ not valid json";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<Person>("person", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNull(result.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenStoredJsonDoesNotMatchType()
    {
        // Arrange - a string value is stored where an int is expected.
        _jsRuntime.Results["localStorage.getItem"] = "\"not-a-number\"";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<int>("age", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(default, result.Value);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenStoredJsonHasWrongShape()
    {
        // Arrange - stored payload is an int, but the caller expects a complex object.
        _jsRuntime.Results["localStorage.getItem"] = "42";
        var service = CreateService();

        // Act
        var result = await service.GetAsync<Person>("person", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNull(result.Value);
    }
}
