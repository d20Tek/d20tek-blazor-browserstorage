namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class CrossTabListenerTests
{   
    private readonly FakeJSRuntime _jsRuntime = new();
    private readonly FakeJSObjectReference _moduleRef = new();

    private LocalStorageService CreateService(string keyPrefix = "")
    {
        _jsRuntime.ModuleReference = _moduleRef;
        _moduleRef.Results["addStorageListener"] = 1;
        var options = Options.Create(new BrowserStorageOptions { KeyPrefix = keyPrefix });
        return new LocalStorageService(_jsRuntime, options);
    }

    [TestMethod]
    public void OnStorageChanged_RaisesChangedEvent()
    {
        // Arrange
        var service = CreateService();
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += (_, e) => eventArgs = e;

        // Act
        service.ListenerManager.OnStorageChanged("myKey", "oldVal", "newVal");

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("myKey", eventArgs.Key);
        Assert.AreEqual("oldVal", eventArgs.OldValue);
        Assert.AreEqual("newVal", eventArgs.NewValue);
    }

    [TestMethod]
    public void OnStorageChanged_WithNullKey_DoesNotRaiseEvent()
    {
        // Arrange
        var service = CreateService();
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += [ExcludeFromCodeCoverage](_, e) => eventArgs = e;

        // Act
        service.ListenerManager.OnStorageChanged(null, "old", "new");

        // Assert
        Assert.IsNull(eventArgs);
    }

    [TestMethod]
    public void OnStorageChanged_StripsPrefix()
    {
        // Arrange
        var service = CreateService("app_");
        StorageChangedEventArgs? eventArgs = null;
        service.Changed += (_, e) => eventArgs = e;

        // Act
        service.ListenerManager.OnStorageChanged("app_myKey", "old", "new");

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("myKey", eventArgs.Key);
    }

    [TestMethod]
    public async Task ChangedSubscription_ImportsJsModule()
    {
        // Arrange
        var service = CreateService();
        Assert.IsFalse(service.ListenerManager.IsInitialized);

        // Act
        service.Changed += [ExcludeFromCodeCoverage](_, _) => { };

        // Allow the fire-and-forget InitializeAsync to complete before asserting IsInitialized.
        await Task.Delay(10, CancellationToken.None);

        // Assert
        Assert.Contains(i => i.Identifier == "import", _jsRuntime.Invocations);
        Assert.IsTrue(service.ListenerManager.IsInitialized);
    }

    [TestMethod]
    public async Task DisposeAsync_RemovesListenerAndDisposesModule()
    {
        // Arrange
        var service = CreateService();
        service.Changed += [ExcludeFromCodeCoverage](_, _) => { };

        // Allow EnsureListenerAsync to complete
        await Task.Delay(10, CancellationToken.None);

        // Act
        await service.DisposeAsync();

        // Assert
        var (Identifier, Args) = _moduleRef.Invocations.First(i => i.Identifier == "removeStorageListener");
        Assert.AreEqual(1, Args[0]);
        Assert.IsTrue(_moduleRef.Disposed);
    }

    [TestMethod]
    public async Task DisposeAsync_WithNoListener_DoesNotThrow()
    {
        // Arrange
        var service = CreateService();

        // Act & Assert - no exception
        await service.DisposeAsync();
    }

    [TestMethod]
    public async Task ChangedSubscription_SecondSubscriber_DoesNotImportModuleAgain()
    {
        // Arrange
        var service = CreateService();
        service.Changed += [ExcludeFromCodeCoverage](_, _) => { };

        // Allow fire-and-forget EnsureListenerAsync to complete
        await Task.Delay(10, CancellationToken.None);

        // Act - subscribe a second handler; accessor sees _module is not null and skips import
        service.Changed += [ExcludeFromCodeCoverage](_, _) => { };
        await Task.Delay(10, CancellationToken.None);

        // Assert - import was only called once
        var importCount = _jsRuntime.Invocations.Count(i => i.Identifier == "import");
        Assert.AreEqual(1, importCount);
    }

    [TestMethod]
    public async Task ChangedSubscription_WritesToConsoleError_WhenModuleImportFails()
    {
        // Arrange
        _jsRuntime.ModuleReference = _moduleRef;
        _jsRuntime.ExceptionForIdentifier["import"] = new InvalidOperationException("module import failed");
        var options = Options.Create(new BrowserStorageOptions());
        var service = new LocalStorageService(_jsRuntime, options);

        var originalError = Console.Error;
        using var captured = new StringWriter();
        Console.SetError(captured);

        try
        {
            // Act
            service.Changed += [ExcludeFromCodeCoverage](_, _) => { };

            // Allow the fire-and-forget continuation to observe the fault.
            await Task.Delay(50, CancellationToken.None);
        }
        finally
        {
            Console.SetError(originalError);
        }

        // Assert
        var output = captured.ToString();
        Assert.Contains("D20Tek.Blazor.BrowserStorage", output);
        Assert.Contains("Failed to initialize storage change listener", output);
        Assert.Contains("module import failed", output);
    }
}
