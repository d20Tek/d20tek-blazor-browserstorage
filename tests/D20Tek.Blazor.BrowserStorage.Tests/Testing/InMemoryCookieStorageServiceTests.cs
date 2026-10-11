namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class InMemoryCookieStorageServiceTests
{
    [TestMethod]
    public async Task SetAsync_ThenGetAsync_RoundTripsValue()
    {
        // Arrange
        var svc = new InMemoryCookieStorageService();

        // Act
        var setResult = await svc.SetAsync("token", "abc123", CancellationToken.None);
        var getResult = await svc.GetAsync<string>("token", CancellationToken.None);

        // Assert
        Assert.IsTrue(setResult.IsSuccess);
        Assert.IsTrue(getResult.IsSuccess);
        Assert.AreEqual("abc123", getResult.Value);
    }

    [TestMethod]
    public void Constructor_WithOptionsInstance_AppliesKeyPrefix()
    {
        // Arrange
        var svc = new InMemoryCookieStorageService(new BrowserStorageOptions { KeyPrefix = "app_" });

        // Act
        svc.Seed("key", "\"val\"");

        // Assert
        Assert.IsTrue(svc.Snapshot.ContainsKey("app_key"));
    }

    [TestMethod]
    public async Task ImplementsICookieStorageService()
    {
        // Arrange
        InMemoryCookieStorageService svc = new();

        // Act
        var result = await svc.SetAsync("k", 1, CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public void ImplementsIInMemoryStorage()
    {
        // Arrange
        var svc = new InMemoryCookieStorageService();

        // Act
        svc.Seed("key", "value");

        // Assert
        IInMemoryStorage storage = svc;
        Assert.ContainsSingle(storage.Snapshot);
    }
}
