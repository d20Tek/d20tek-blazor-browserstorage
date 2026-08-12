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
        service.OnStorageChanged("myKey", "oldVal", "newVal");

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
        service.OnStorageChanged(null, "old", "new");

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
        service.OnStorageChanged("app_myKey", "old", "new");

        // Assert
        Assert.IsNotNull(eventArgs);
        Assert.AreEqual("myKey", eventArgs.Key);
    }

    [TestMethod]
    public void ChangedSubscription_ImportsJsModule()
    {
        // Arrange
        var service = CreateService();

        // Act
        service.Changed += [ExcludeFromCodeCoverage](_, _) => { };

        // Assert
        Assert.Contains(i => i.Identifier == "import", _jsRuntime.Invocations);
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
}
