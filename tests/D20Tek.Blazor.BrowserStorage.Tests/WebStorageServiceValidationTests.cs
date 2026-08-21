using Microsoft.Extensions.DependencyInjection;

namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class WebStorageServiceValidationTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    private LocalStorageService CreateService() => new(_jsRuntime, Options.Create(new BrowserStorageOptions()));

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task GetAsync_ThrowsArgumentException_WhenKeyIsNullOrEmpty(string? key)
    {
        // Arrange
        var service = CreateService();

        // Act - Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.GetAsync<int>(key!, CancellationToken.None));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task SetAsync_ThrowsArgumentException_WhenKeyIsNullOrEmpty(string? key)
    {
        // Arrange
        var service = CreateService();

        // Act - Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.SetAsync(key!, "value", CancellationToken.None));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task RemoveAsync_ThrowsArgumentException_WhenKeyIsNullOrEmpty(string? key)
    {
        // Arrange
        var service = CreateService();

        // Act - Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.RemoveAsync(key!, CancellationToken.None));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public async Task ContainsKeyAsync_ThrowsArgumentException_WhenKeyIsNullOrEmpty(string? key)
    {
        // Arrange
        var service = CreateService();

        // Act - Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await service.ContainsKeyAsync(key!, CancellationToken.None));
    }

    [TestMethod]
    public async Task SetMultipleAsync_ThrowsArgumentNullException_WhenServiceIsNull()
    {
        // Arrange
        IBrowserStorageService service = null!;
        var items = new Dictionary<string, object>();

        // Act - Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await service.SetMultipleAsync(items, CancellationToken.None));
    }

    [TestMethod]
    public async Task SetMultipleAsync_ThrowsArgumentNullException_WhenItemsIsNull()
    {
        // Arrange
        var service = CreateService();
        IEnumerable<KeyValuePair<string, object>> items = null!;

        // Act - Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await service.SetMultipleAsync(items, CancellationToken.None));
    }

    [TestMethod]
    public async Task RemoveMultipleAsync_ThrowsArgumentNullException_WhenServiceIsNull()
    {
        // Arrange
        IBrowserStorageService service = null!;
        var keys = Array.Empty<string>();

        // Act - Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await service.RemoveMultipleAsync(keys, CancellationToken.None));
    }

    [TestMethod]
    public async Task RemoveMultipleAsync_ThrowsArgumentNullException_WhenKeysIsNull()
    {
        // Arrange
        var service = CreateService();
        IEnumerable<string> keys = null!;

        // Act - Assert
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(
            async () => await service.RemoveMultipleAsync(keys, CancellationToken.None));
    }

    [TestMethod]
    public void AddBrowserStorage_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act - Assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => services.AddBrowserStorage());
    }

    [TestMethod]
    public void AddLocalStorage_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act - Assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => services.AddLocalStorage());
    }

    [TestMethod]
    public void AddSessionStorage_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        // Arrange
        IServiceCollection services = null!;

        // Act - Assert
        Assert.ThrowsExactly<ArgumentNullException>([ExcludeFromCodeCoverage]() => services.AddSessionStorage());
    }
}
