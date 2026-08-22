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
        var svc = CreateLocal();
        var value = new Prefs("Dark", 14);

        var setResult = await svc.SetAsync("prefs", value);
        var getResult = await svc.GetAsync<Prefs>("prefs");

        Assert.IsTrue(setResult.IsSuccess);
        Assert.IsTrue(getResult.IsSuccess);
        Assert.AreEqual(value, getResult.Value);
    }

    [TestMethod]
    public async Task GetAsync_MissingKey_ReturnsFailure()
    {
        var svc = CreateLocal();

        var result = await svc.GetAsync<Prefs>("missing");

        Assert.IsFalse(result.IsSuccess);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    public async Task RemoveAsync_RemovesKey()
    {
        var svc = CreateLocal();
        await svc.SetAsync("k", 42);

        var result = await svc.RemoveAsync("k");

        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(await svc.ContainsKeyAsync("k"));
    }

    [TestMethod]
    public async Task ClearAllAsync_RemovesEverything()
    {
        var svc = CreateLocal();
        await svc.SetAsync("a", 1);
        await svc.SetAsync("b", 2);

        var result = await svc.ClearAllAsync();

        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, await svc.LengthAsync());
    }

    [TestMethod]
    public async Task GetKeysAsync_StripsKeyPrefix()
    {
        var svc = CreateLocal("app_");
        await svc.SetAsync("one", 1);
        await svc.SetAsync("two", 2);

        var keys = await svc.GetKeysAsync();

        CollectionAssert.AreEquivalent(new[] { "one", "two" }, keys.ToArray());
    }

    [TestMethod]
    public void Seed_AppliesKeyPrefix_AndDoesNotRaiseChanged()
    {
        var svc = CreateLocal("app_");
        IInMemoryStorage store = svc;
        var raised = 0;
        svc.Changed += [ExcludeFromCodeCoverage](_, _) => raised++;

        store.Seed("k", 42);

        Assert.IsTrue(store.Snapshot.ContainsKey("app_k"));
        Assert.AreEqual(0, raised);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public async Task SetAsync_RaisesChanged_WithOldAndNewValues()
    {
        var svc = CreateLocal();
        await svc.SetAsync("k", 1);

        StorageChangedEventArgs? captured = null;
        svc.Changed += (_, e) => captured = e;

        await svc.SetAsync("k", 2);

        Assert.IsNotNull(captured);
        Assert.AreEqual("k", captured!.Key);
        Assert.AreEqual(2, captured.NewValue is JsonElement je ? je.GetInt32() : (int)captured.NewValue!);
    }

    [TestMethod]
    public async Task SimulateUnavailable_MakesOperationsFail()
    {
        var svc = CreateLocal();
        IInMemoryStorage store = svc;
        await svc.SetAsync("k", 1);

        store.SimulateUnavailable();

        Assert.IsFalse(await svc.IsAvailableAsync());
        Assert.IsFalse((await svc.GetAsync<int>("k")).IsSuccess);
        Assert.IsFalse((await svc.SetAsync("k", 2)).IsSuccess);
        Assert.IsFalse(await svc.ContainsKeyAsync("k"));

        store.RestoreAvailable();

        Assert.IsTrue(await svc.IsAvailableAsync());
        Assert.IsTrue((await svc.GetAsync<int>("k")).IsSuccess);
    }

    [TestMethod]
    public void RaiseExternalChange_FiresChangedEvent()
    {
        var svc = CreateLocal();
        IInMemoryStorage store = svc;
        StorageChangedEventArgs? captured = null;
        svc.Changed += (_, e) => captured = e;

        store.RaiseExternalChange("k", oldValue: "1", newValue: "2");

        Assert.IsNotNull(captured);
        Assert.AreEqual("k", captured!.Key);
    }
}
