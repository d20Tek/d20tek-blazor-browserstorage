namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class BrowserStorageServiceBulkExtensionsTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService(string keyPrefix = "")
    {
        var options = Options.Create(new BrowserStorageOptions { KeyPrefix = keyPrefix });
        return new LocalStorageService(_jsRuntime, options);
    }

    // --- SetMultipleAsync ---

    [TestMethod]
    public async Task SetMultipleAsync_SetsAllItems()
    {
        // Arrange
        var service = CreateService();
        var items = new Dictionary<string, object>
        {
            ["a"] = "val1",
            ["b"] = 42
        };

        // Act
        await service.SetMultipleAsync(items, CancellationToken.None);

        // Assert
        var setInvocations = _jsRuntime.Invocations.Where(i => i.Identifier == "localStorage.setItem").ToList();
        Assert.HasCount(2, setInvocations);
    }

    [TestMethod]
    public async Task SetMultipleAsync_UsesKeyPrefix()
    {
        // Arrange
        var service = CreateService("p_");
        var items = new Dictionary<string, object> { ["x"] = "y" };

        // Act
        await service.SetMultipleAsync(items, CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.setItem");
        Assert.AreEqual("p_x", Args[0]);
    }

    // --- RemoveMultipleAsync ---

    [TestMethod]
    public async Task RemoveMultipleAsync_RemovesAllKeys()
    {
        // Arrange
        var service = CreateService();

        // Act
        await service.RemoveMultipleAsync(["a", "b", "c"], CancellationToken.None);

        // Assert
        var removeInvocations = _jsRuntime.Invocations.Where(i => i.Identifier == "localStorage.removeItem").ToList();
        Assert.HasCount(3, removeInvocations);
    }

    [TestMethod]
    public async Task RemoveMultipleAsync_UsesKeyPrefix()
    {
        // Arrange
        var service = CreateService("ns_");

        // Act
        await service.RemoveMultipleAsync(["key"], CancellationToken.None);

        // Assert
        var (Identifier, Args) = _jsRuntime.Invocations.First(i => i.Identifier == "localStorage.removeItem");
        Assert.AreEqual("ns_key", Args[0]);
    }
}
