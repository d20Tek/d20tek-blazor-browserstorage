using Microsoft.Extensions.DependencyInjection;

namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class InMemoryCookieStorageServiceCollectionExtensionsTests
{
    [TestMethod]
    public async Task AddInMemoryCookieStorage_RegistersCookieService_AndAppliesOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInMemoryCookieStorage(o => o.KeyPrefix = "app_");
        var sp = services.BuildServiceProvider();

        // Assert
        var cookie = sp.GetRequiredService<ICookieStorageService>();
        Assert.IsNotNull(cookie);
        _ = await cookie.SetAsync("k", 1, CancellationToken.None);
        Assert.IsTrue(sp.GetInMemoryCookieStorage().Snapshot.ContainsKey("app_k"));
    }

    [TestMethod]
    public void AddInMemoryCookieStorage_OnlyRegistersCookie()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInMemoryCookieStorage();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(sp.GetService<ICookieStorageService>());
        Assert.IsNull(sp.GetService<ILocalStorageService>());
        Assert.IsNull(sp.GetService<ISessionStorageService>());
    }

    [TestMethod]
    public void GetInMemoryCookieStorage_ReturnsDistinctInstance_FromLocalAndSession()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddInMemoryBrowserStorage();
        services.AddInMemoryCookieStorage();

        // Act
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.AreNotSame(sp.GetInMemoryCookieStorage(), sp.GetInMemoryLocalStorage());
        Assert.AreNotSame(sp.GetInMemoryCookieStorage(), sp.GetInMemorySessionStorage());
    }

    [TestMethod]
    public async Task ReplaceWithInMemoryCookieStorage_RemovesPriorRegistration_AndAddsInMemory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ICookieStorageService, SentinelCookieStorage>();

        // Act
        services.ReplaceWithInMemoryCookieStorage(o => o.KeyPrefix = "app_");
        var sp = services.BuildServiceProvider();

        // Assert
        var cookie = sp.GetRequiredService<ICookieStorageService>();
        Assert.IsInstanceOfType<InMemoryCookieStorageService>(cookie);
        _ = await cookie.SetAsync("k", 1, CancellationToken.None);
        Assert.IsTrue(sp.GetInMemoryCookieStorage().Snapshot.ContainsKey("app_k"));
    }

    [TestMethod]
    public void ReplaceWithInMemoryCookieStorage_WorksWhenNoPriorRegistrationExists()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ReplaceWithInMemoryCookieStorage();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<InMemoryCookieStorageService>(sp.GetRequiredService<ICookieStorageService>());
    }

    [TestMethod]
    public void ReplaceWithInMemoryCookieStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceCollection)null!).ReplaceWithInMemoryCookieStorage());

    [TestMethod]
    public void AddInMemoryCookieStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceCollection)null!).AddInMemoryCookieStorage());

    [TestMethod]
    public void GetInMemoryCookieStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceProvider)null!).GetInMemoryCookieStorage());
}

[ExcludeFromCodeCoverage]
file sealed class SentinelCookieStorage : ICookieStorageService
{
    public event EventHandler<StorageChangedEventArgs>? Changed { add { } remove { } }
    public ValueTask<bool> IsAvailableAsync(CancellationToken cancellationToken = default) => new(false);
    public ValueTask<StorageResult<T>> GetAsync<T>(string key, CancellationToken cancellationToken = default) => new(StorageResult<T>.Failure("sentinel"));
    public ValueTask<StorageResult> SetAsync<T>(string key, T value, CancellationToken cancellationToken = default) => new(StorageResult.Failure("sentinel"));
    public ValueTask<StorageResult> RemoveAsync(string key, CancellationToken cancellationToken = default) => new(StorageResult.Failure("sentinel"));
    public ValueTask<StorageResult> ClearAllAsync(CancellationToken cancellationToken = default) => new(StorageResult.Failure("sentinel"));
    public ValueTask<bool> ContainsKeyAsync(string key, CancellationToken cancellationToken = default) => new(false);
    public ValueTask<int> LengthAsync(CancellationToken cancellationToken = default) => new(0);
    public ValueTask<IReadOnlyList<string>> GetKeysAsync(CancellationToken cancellationToken = default) => new([]);
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
