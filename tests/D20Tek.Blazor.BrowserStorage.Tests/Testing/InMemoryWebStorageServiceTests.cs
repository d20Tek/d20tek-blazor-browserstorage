namespace D20Tek.Blazor.BrowserStorage.Tests.Testing;

[TestClass]
public class InMemoryWebStorageServiceTests
{
    private sealed record Prefs(string Theme, int FontSize);

    private static InMemoryLocalStorageService CreateLocal(string prefix = "") =>
        new(Options.Create(new BrowserStorageOptions { KeyPrefix = prefix }));

    [TestMethod]
    public async Task SetAsync_ThenGetAsync_RoundTripsValue()
    {
        // Arrange
        var svc = CreateLocal();
        var value = new Prefs("Dark", 14);

        // Act
        var setResult = await svc.SetAsync("prefs", value, CancellationToken.None);
        var getResult = await svc.GetAsync<Prefs>("prefs", CancellationToken.None);

        // Assert
        Assert.IsTrue(setResult.IsSuccess);
        Assert.IsTrue(getResult.IsSuccess);
        Assert.AreEqual(value, getResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_MissingKey_ReturnsFailure()
    {
        // Arrange
        var svc = CreateLocal();

        // Act
        var result = await svc.GetAsync<Prefs>("missing", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task RemoveAsync_RemovesKey()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 42, CancellationToken.None);

        // Act
        var result = await svc.RemoveAsync("k", CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(await svc.ContainsKeyAsync("k", CancellationToken.None));
    }

    [TestMethod]
    public async Task ClearAllAsync_RemovesEverything()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);

        // Act
        var result = await svc.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, await svc.LengthAsync(CancellationToken.None));
    }

    [TestMethod]
    public async Task GetKeysAsync_StripsKeyPrefix()
    {
        // Arrange
        var svc = CreateLocal("app_");
        await svc.SetAsync("one", 1, CancellationToken.None);
        await svc.SetAsync("two", 2, CancellationToken.None);

        // Act
        var keys = await svc.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.AreSequenceEqual(["one", "two"], [.. keys], SequenceOrder.InAnyOrder);
    }

    [TestMethod]
    public void Seed_AppliesKeyPrefix_AndDoesNotRaiseChanged()
    {
        // Arrange
        var svc = CreateLocal("app_");
        using var _ = svc.RecordChanges(out var events);

        // Act
        ((IInMemoryStorage)svc).Seed("k", 42);

        // Assert
        Assert.IsTrue(svc.Snapshot.ContainsKey("app_k"));
        Assert.IsEmpty(events);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public async Task SetAsync_RaisesChanged_WithOldAndNewValues()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);
        using var _ = svc.RecordChanges(out var events);

        // Act
        await svc.SetAsync("k", 2, CancellationToken.None);

        // Assert
        Assert.HasCount(1, events);
        var captured = events[0];
        Assert.AreEqual("k", captured.Key);
        Assert.AreEqual(2, captured.NewValue is JsonElement je ? je.GetInt32() : (int)captured.NewValue!);
    }

    [TestMethod]
    public async Task SimulateUnavailable_MakesOperationsFail()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);

        // Act
        svc.SimulateUnavailable();

        // Assert
        Assert.IsFalse(await svc.IsAvailableAsync(CancellationToken.None));
        Assert.IsFalse((await svc.GetAsync<int>("k", CancellationToken.None)).IsSuccess);
        Assert.IsFalse((await svc.SetAsync("k", 2, CancellationToken.None)).IsSuccess);
        Assert.IsFalse(await svc.ContainsKeyAsync("k", CancellationToken.None));

        // Act
        svc.RestoreAvailable();

        // Assert
        Assert.IsTrue(await svc.IsAvailableAsync(CancellationToken.None));
        Assert.IsTrue((await svc.GetAsync<int>("k", CancellationToken.None)).IsSuccess);
    }

    [TestMethod]
    public void RaiseExternalChange_FiresChangedEvent()
    {
        // Arrange
        var svc = CreateLocal();
        using var _ = svc.RecordChanges(out var events);

        // Act
        svc.RaiseExternalChange("k", oldValue: "1", newValue: "2");

        // Assert
        Assert.HasCount(1, events);
        Assert.AreEqual("k", events[0].Key);
    }

    [TestMethod]
    public void Clear_RemovesAllEntriesWithoutRaisingChanged()
    {
        // Arrange
        var svc = CreateLocal();
        ((IInMemoryStorage)svc).Seed("a", 1);
        ((IInMemoryStorage)svc).Seed("b", 2);
        using var _ = svc.RecordChanges(out var events);

        // Act
        ((IInMemoryStorage)svc).Clear();

        // Assert
        Assert.IsEmpty(((IInMemoryStorage)svc).Snapshot);
        Assert.HasCount(0, events);
    }

    [TestMethod]
    public void Seed_BulkRaw_WritesAllEntries_AndDoesNotRaiseChanged()
    {
        // Arrange
        var svc = CreateLocal("app_");
        using var _ = svc.RecordChanges(out var events);
        var entries = new Dictionary<string, string>
        {
            ["a"] = "1",
            ["b"] = "\"hello\"",
        };

        // Act
        svc.Seed(entries);

        // Assert
        Assert.HasCount(2, svc.Snapshot);
        Assert.AreEqual("1", svc.Snapshot["app_a"]);
        Assert.AreEqual("\"hello\"", svc.Snapshot["app_b"]);
        Assert.HasCount(0, events);
    }

    [TestMethod]
    public void Seed_BulkRaw_ThrowsArgumentNullException_WhenEntriesIsNull()
    {
        // Arrange
        var svc = CreateLocal();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(
            [ExcludeFromCodeCoverage]() => svc.Seed((IEnumerable<KeyValuePair<string, string>>)null!));
    }

    [TestMethod]
    public void Seed_BulkTyped_WritesAllEntries()
    {
        // Arrange
        var svc = CreateLocal();

        // Act
        ((IInMemoryStorage)svc).Seed(new Dictionary<string, int> { ["a"] = 1, ["b"] = 2 });

        // Assert
        Assert.HasCount(2, svc.Snapshot);
    }

    [TestMethod]
    public async Task RecordChanges_CapturesEventsUntilDisposed()
    {
        // Arrange
        var svc = CreateLocal();

        // Act & Assert (inside scope)
        using (svc.RecordChanges(out var events))
        {
            await svc.SetAsync("k", 1, CancellationToken.None);
            await svc.SetAsync("k", 2, CancellationToken.None);

            Assert.HasCount(2, events);
            Assert.AreEqual("k", events[0].Key);
        }

        // Act (post-dispose write should not throw or be captured by the disposed handle)
        await svc.SetAsync("k", 3, CancellationToken.None);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);
        svc.SimulateUnavailable();

        // Act
        var result = await svc.GetAsync<int>("k", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task SetAsync_ReturnsFailure_AndDoesNotRaiseChanged_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        svc.SimulateUnavailable();
        using var _ = svc.RecordChanges(out var events);

        // Act
        var result = await svc.SetAsync("k", 1, CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.HasCount(0, events);
        Assert.HasCount(0, svc.Snapshot);
    }

    [TestMethod]
    public async Task RemoveAsync_ReturnsFailure_AndDoesNotRaiseChanged_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);
        svc.SimulateUnavailable();
        using var _ = svc.RecordChanges(out var events);

        // Act
        var result = await svc.RemoveAsync("k", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.HasCount(0, events);
        Assert.HasCount(1, svc.Snapshot);
    }

    [TestMethod]
    public async Task ClearAllAsync_ReturnsFailure_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);
        svc.SimulateUnavailable();

        // Act
        var result = await svc.ClearAllAsync(CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.HasCount(1, svc.Snapshot);
    }

    [TestMethod]
    public async Task ContainsKeyAsync_ReturnsFalse_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);
        svc.SimulateUnavailable();

        // Act
        var contains = await svc.ContainsKeyAsync("k", CancellationToken.None);

        // Assert
        Assert.IsFalse(contains);
    }

    [TestMethod]
    public async Task LengthAsync_ReturnsZero_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("a", 1, CancellationToken.None);
        await svc.SetAsync("b", 2, CancellationToken.None);
        svc.SimulateUnavailable();

        // Act
        var length = await svc.LengthAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual(0, length);
    }

    [TestMethod]
    public async Task GetKeysAsync_ReturnsEmpty_WhenUnavailable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("a", 1, CancellationToken.None);
        svc.SimulateUnavailable();

        // Act
        var keys = await svc.GetKeysAsync(CancellationToken.None);

        // Assert
        Assert.IsEmpty(keys);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenStoredJsonIsMalformed()
    {
        // Arrange
        var svc = CreateLocal();
        svc.Seed("k", "not-json");

        // Act
        var result = await svc.GetAsync<int>("k", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task GetAsync_ReturnsFailure_WhenStoredJsonHasWrongShape()
    {
        // Arrange
        var svc = CreateLocal();
        svc.Seed("prefs", "{\"Theme\":123}");

        // Act
        var result = await svc.GetAsync<Prefs>("prefs", CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task DisposeAsync_CompletesSynchronously_AndLeavesfServiceUsable()
    {
        // Arrange
        var svc = CreateLocal();
        await svc.SetAsync("k", 1, CancellationToken.None);

        // Act
        var task = svc.DisposeAsync();

        // Assert
        Assert.IsTrue(task.IsCompletedSuccessfully);
        await task;
        // The in-memory store holds no unmanaged resources, so the service remains usable.
        Assert.IsTrue((await svc.GetAsync<int>("k", CancellationToken.None)).IsSuccess);
    }

    [TestMethod]
    public async Task IsAvailableAsync_ReturnsFalse_AfterSimulateUnavailable()
    {
        // Arrange
        var svc = CreateLocal();

        // Act & Assert
        Assert.IsTrue(await svc.IsAvailableAsync(CancellationToken.None));
        svc.SimulateUnavailable();
        Assert.IsFalse(await svc.IsAvailableAsync(CancellationToken.None));
        svc.RestoreAvailable();
        Assert.IsTrue(await svc.IsAvailableAsync(CancellationToken.None));
    }
}
