using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class BrowserStorageTestContextExtensionsTests
{
    [TestMethod]
    public void AddBrowserStorage_RegistersLocalAndSessionServices()
    {
        using var ctx = new BunitContext();

        ctx.AddBrowserStorage();

        Assert.IsNotNull(ctx.Services.GetService<ILocalStorageService>());
        Assert.IsNotNull(ctx.Services.GetService<ISessionStorageService>());
        Assert.IsNotNull(ctx.GetLocalStorage());
        Assert.IsNotNull(ctx.GetSessionStorage());
    }

    [TestMethod]
    public void AddBrowserStorage_AppliesOptionsConfiguration()
    {
        using var ctx = new BunitContext();

        ctx.AddBrowserStorage(o => o.KeyPrefix = "app_");
        ctx.GetLocalStorage().Seed("k", "\"v\"");

        Assert.IsTrue(ctx.GetLocalStorage().Snapshot.ContainsKey("app_k"));
    }

    [TestMethod]
    public async Task LocalAndSessionStorage_AreIndependent()
    {
        using var ctx = new BunitContext();
        ctx.AddBrowserStorage();

        var local = ctx.Services.GetRequiredService<ILocalStorageService>();
        var session = ctx.Services.GetRequiredService<ISessionStorageService>();

        await local.SetAsync("k", 1);

        Assert.IsTrue(await local.ContainsKeyAsync("k"));
        Assert.IsFalse(await session.ContainsKeyAsync("k"));
    }

    [TestMethod]
    public void AddLocalStorage_OnlyRegistersLocal()
    {
        using var ctx = new BunitContext();

        ctx.AddLocalStorage();

        Assert.IsNotNull(ctx.Services.GetService<ILocalStorageService>());
        Assert.IsNull(ctx.Services.GetService<ISessionStorageService>());
    }

    [TestMethod]
    public void AddSessionStorage_OnlyRegistersSession()
    {
        // Arrange
        using var ctx = new BunitContext();

        // Act
        ctx.AddSessionStorage(o => o.KeyPrefix = "s_");

        // Assert
        Assert.IsNotNull(ctx.Services.GetService<ISessionStorageService>());
        Assert.IsNull(ctx.Services.GetService<ILocalStorageService>());
        ctx.GetSessionStorage().Seed("k", "\"v\"");
        Assert.IsTrue(ctx.GetSessionStorage().Snapshot.ContainsKey("s_k"));
    }

    [TestMethod]
    public void AddSessionStorage_ThrowsArgumentNullException_WhenContextIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((BunitContext)null!).AddSessionStorage());
}
