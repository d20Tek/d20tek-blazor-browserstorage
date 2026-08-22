using Microsoft.Extensions.DependencyInjection;

namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class InMemoryBrowserStorageServiceCollectionExtensionsTests
{
    [TestMethod]
    public async Task AddInMemoryBrowserStorage_RegistersBothServices_AndAppliesOptions()
    {
        var services = new ServiceCollection();
        services.AddInMemoryBrowserStorage(o => o.KeyPrefix = "app_");
        var sp = services.BuildServiceProvider();

        var local = sp.GetRequiredService<ILocalStorageService>();
        var session = sp.GetRequiredService<ISessionStorageService>();

        Assert.IsNotNull(local);
        Assert.IsNotNull(session);
        _ = await local.SetAsync("k", 1, CancellationToken.None);
        Assert.IsTrue(sp.GetInMemoryLocalStorage().Snapshot.ContainsKey("app_k"));
    }

    [TestMethod]
    public void GetInMemoryLocalStorage_And_GetInMemorySessionStorage_ReturnDistinctInstances()
    {
        var services = new ServiceCollection();
        services.AddInMemoryBrowserStorage();
        var sp = services.BuildServiceProvider();

        Assert.AreNotSame(sp.GetInMemoryLocalStorage(), sp.GetInMemorySessionStorage());
    }

    [TestMethod]
    public void AddInMemoryLocalStorage_OnlyRegistersLocal()
    {
        var services = new ServiceCollection();
        services.AddInMemoryLocalStorage();
        var sp = services.BuildServiceProvider();

        Assert.IsNotNull(sp.GetService<ILocalStorageService>());
        Assert.IsNull(sp.GetService<ISessionStorageService>());
    }

    [TestMethod]
    public async Task ReplaceWithInMemoryBrowserStorage_RemovesPriorRegistrations_AndAddsInMemory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILocalStorageService, SentinelLocalStorage>();
        services.AddSingleton<ISessionStorageService, SentinelSessionStorage>();

        // Act
        services.ReplaceWithInMemoryBrowserStorage(o => o.KeyPrefix = "app_");
        var sp = services.BuildServiceProvider();

        // Assert
        var local = sp.GetRequiredService<ILocalStorageService>();
        var session = sp.GetRequiredService<ISessionStorageService>();
        Assert.IsInstanceOfType<InMemoryLocalStorageService>(local);
        Assert.IsInstanceOfType<InMemorySessionStorageService>(session);
        _ = await local.SetAsync("k", 1, CancellationToken.None);
        Assert.IsTrue(sp.GetInMemoryLocalStorage().Snapshot.ContainsKey("app_k"));
    }

    [TestMethod]
    public void ReplaceWithInMemoryBrowserStorage_WorksWhenNoPriorRegistrationExists()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.ReplaceWithInMemoryBrowserStorage();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<InMemoryLocalStorageService>(sp.GetRequiredService<ILocalStorageService>());
        Assert.IsInstanceOfType<InMemorySessionStorageService>(sp.GetRequiredService<ISessionStorageService>());
    }

    [TestMethod]
    public void ReplaceWithInMemoryLocalStorage_RemovesPriorLocal_AndPreservesSession()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILocalStorageService, SentinelLocalStorage>();
        services.AddSingleton<ISessionStorageService, SentinelSessionStorage>();

        // Act
        services.ReplaceWithInMemoryLocalStorage();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<InMemoryLocalStorageService>(sp.GetRequiredService<ILocalStorageService>());
        Assert.IsInstanceOfType<SentinelSessionStorage>(sp.GetRequiredService<ISessionStorageService>());
    }

    [TestMethod]
    public void ReplaceWithInMemorySessionStorage_RemovesPriorSession_AndPreservesLocal()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ILocalStorageService, SentinelLocalStorage>();
        services.AddSingleton<ISessionStorageService, SentinelSessionStorage>();

        // Act
        services.ReplaceWithInMemorySessionStorage();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.IsInstanceOfType<InMemorySessionStorageService>(sp.GetRequiredService<ISessionStorageService>());
        Assert.IsInstanceOfType<SentinelLocalStorage>(sp.GetRequiredService<ILocalStorageService>());
    }

    [TestMethod]
    public void ReplaceWithInMemoryBrowserStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceCollection)null!).ReplaceWithInMemoryBrowserStorage());

    [TestMethod]
    public void ReplaceWithInMemoryLocalStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceCollection)null!).ReplaceWithInMemoryLocalStorage());

    [TestMethod]
    public void ReplaceWithInMemorySessionStorage_ThrowsArgumentNullException_WhenServicesIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IServiceCollection)null!).ReplaceWithInMemorySessionStorage());
}

[ExcludeFromCodeCoverage]
file sealed class SentinelLocalStorage : ILocalStorageService
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

[ExcludeFromCodeCoverage]
file sealed class SentinelSessionStorage : ISessionStorageService
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

[TestClass]
public class InMemoryStorageConvenienceConstructorTests
{
    [TestMethod]
    public async Task Parameterless_Constructor_UsesDefaultOptions()
    {
        var svc = new InMemoryLocalStorageService();

        await svc.SetAsync("k", 42, CancellationToken.None);

        Assert.IsTrue(((IInMemoryStorage)svc).Snapshot.ContainsKey("k"));
    }

    [TestMethod]
    public async Task Options_Constructor_AppliesKeyPrefix()
    {
        var svc = new InMemorySessionStorageService(new BrowserStorageOptions { KeyPrefix = "s_" });

        await svc.SetAsync("k", "v", CancellationToken.None);

        Assert.IsTrue(((IInMemoryStorage)svc).Snapshot.ContainsKey("s_k"));
    }

    [TestMethod]
    public async Task Session_Parameterless_Constructor_UsesDefaultOptions()
    {
        // Arrange
        var svc = new InMemorySessionStorageService();

        // Act
        await svc.SetAsync("k", 42, CancellationToken.None);

        // Assert
        Assert.IsTrue(((IInMemoryStorage)svc).Snapshot.ContainsKey("k"));
    }
}
