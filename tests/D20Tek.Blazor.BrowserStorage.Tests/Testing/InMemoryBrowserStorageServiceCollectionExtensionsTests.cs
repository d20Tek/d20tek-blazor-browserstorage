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
        await local.SetAsync("k", 1);
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
}

[TestClass]
public class InMemoryStorageConvenienceConstructorTests
{
    [TestMethod]
    public async Task Parameterless_Constructor_UsesDefaultOptions()
    {
        var svc = new InMemoryLocalStorageService();

        await svc.SetAsync("k", 42);

        Assert.IsTrue(((IInMemoryStorage)svc).Snapshot.ContainsKey("k"));
    }

    [TestMethod]
    public async Task Options_Constructor_AppliesKeyPrefix()
    {
        var svc = new InMemorySessionStorageService(new BrowserStorageOptions { KeyPrefix = "s_" });

        await svc.SetAsync("k", "v");

        Assert.IsTrue(((IInMemoryStorage)svc).Snapshot.ContainsKey("s_k"));
    }
}
