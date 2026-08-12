using D20Tek.Blazor.BrowserStorage.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddBrowserStorage_RegistersBothServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJSRuntime());

        // Act
        services.AddBrowserStorage();

        // Assert
        var provider = services.BuildServiceProvider();
        Assert.IsNotNull(provider.GetService<ILocalStorageService>());
        Assert.IsNotNull(provider.GetService<ISessionStorageService>());
    }

    [TestMethod]
    public void AddLocalStorage_RegistersLocalStorageService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJSRuntime());

        // Act
        services.AddLocalStorage();

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ILocalStorageService>();
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void AddSessionStorage_RegistersSessionStorageService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJSRuntime());

        // Act
        services.AddSessionStorage();

        // Assert
        var provider = services.BuildServiceProvider();
        var service = provider.GetService<ISessionStorageService>();
        Assert.IsNotNull(service);
    }

    [TestMethod]
    public void AddBrowserStorage_WithOptions_ConfiguresPrefix()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJSRuntime());

        // Act
        services.AddBrowserStorage(opts => opts.KeyPrefix = "test_");

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BrowserStorageOptions>>();
        Assert.AreEqual("test_", options.Value.KeyPrefix);
    }

    [TestMethod]
    public void AddLocalStorage_WithoutOptions_UsesDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJSRuntime());

        // Act
        services.AddLocalStorage();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<BrowserStorageOptions>>();
        Assert.AreEqual(string.Empty, options.Value.KeyPrefix);
    }
}
