namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class BrowserStorageChangeRecorderExtensionsTests
{
    private static InMemoryLocalStorageService CreateLocal() => new(Options.Create(new BrowserStorageOptions()));

    [TestMethod]
    public async Task RecordChanges_Count_ReflectsCapturedEvents()
    {
        // Arrange
        var svc = CreateLocal();
        using var _ = svc.RecordChanges(out var events);

        // Act
        Assert.AreEqual(0, events.Count);
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);
        await svc.SetAsync("c", 3, CancellationToken.None);

        // Assert
        Assert.AreEqual(3, events.Count);
    }

    [TestMethod]
    public async Task RecordChanges_Indexer_ReturnsEventInInsertionOrder()
    {
        // Arrange
        var svc = CreateLocal();
        using var _ = svc.RecordChanges(out var events);

        // Act
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);

        // Assert
        Assert.AreEqual("a", events[0].Key);
        Assert.AreEqual("b", events[1].Key);
    }

    [TestMethod]
    public async Task RecordChanges_Enumeration_YieldsSnapshotOfCurrentEvents()
    {
        // Arrange
        var svc = CreateLocal();
        using var _ = svc.RecordChanges(out var events);
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);

        // Act
        var keys = events.Select(e => e.Key).ToArray();

        // Assert
        Assert.AreSequenceEqual(["a", "b"], keys);
    }

    [TestMethod]
    public async Task RecordChanges_Dispose_UnsubscribesHandler()
    {
        // Arrange
        var svc = CreateLocal();
        IReadOnlyList<StorageChangedEventArgs> events;
        using (svc.RecordChanges(out events))
        {
            await svc.SetAsync("k", 1, CancellationToken.None);
        }

        // Act
        await svc.SetAsync("k", 2, CancellationToken.None);

        // Assert
        Assert.AreEqual(1, events.Count);
    }

    [TestMethod]
    public void RecordChanges_ThrowsArgumentNullException_WhenServiceIsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => ((IBrowserStorageService)null!).RecordChanges(out _));

    [TestMethod]
    public async Task RecordChanges_NonGenericEnumeration_YieldsCapturedEvents()
    {
        // Arrange
        var svc = CreateLocal();
        using var _ = svc.RecordChanges(out var events);
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);

        // Act
        var keys = new List<string>();
        foreach (object? item in (System.Collections.IEnumerable)events)
        {
            keys.Add(((StorageChangedEventArgs)item!).Key);
        }

        // Assert
        Assert.AreSequenceEqual(["a", "b"], keys);
    }
}
