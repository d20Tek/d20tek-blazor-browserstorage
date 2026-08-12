namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class JsInteropTests
{
    private readonly FakeJSRuntime _jsRuntime = new();

    [TestMethod]
    public async Task GetItemAsync_InvokesCorrectJsMethod()
    {
        // Arrange
        _jsRuntime.Results["localStorage.getItem"] = "test-value";

        // Act
        var result = await JsInterop.GetItemAsync(_jsRuntime, "localStorage", "myKey", CancellationToken.None);

        // Assert
        Assert.AreEqual("test-value", result);
        Assert.HasCount(1, _jsRuntime.Invocations);
        Assert.AreEqual("localStorage.getItem", _jsRuntime.Invocations[0].Identifier);
        Assert.AreEqual("myKey", _jsRuntime.Invocations[0].Args[0]);
    }

    [TestMethod]
    public async Task GetItemAsync_ReturnsNull_WhenKeyNotFound()
    {
        // Act
        var result = await JsInterop.GetItemAsync(_jsRuntime, "localStorage", "missing", CancellationToken.None);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task SetItemAsync_InvokesCorrectJsMethod()
    {
        // Act
        await JsInterop.SetItemAsync(_jsRuntime, "localStorage", "myKey", "{\"value\":1}", CancellationToken.None);

        // Assert
        Assert.HasCount(1, _jsRuntime.Invocations);
        Assert.AreEqual("localStorage.setItem", _jsRuntime.Invocations[0].Identifier);
        Assert.AreEqual("myKey", _jsRuntime.Invocations[0].Args[0]);
        Assert.AreEqual("{\"value\":1}", _jsRuntime.Invocations[0].Args[1]);
    }

    [TestMethod]
    public async Task RemoveItemAsync_InvokesCorrectJsMethod()
    {
        // Act
        await JsInterop.RemoveItemAsync(_jsRuntime, "sessionStorage", "myKey", CancellationToken.None);

        // Assert
        Assert.HasCount(1, _jsRuntime.Invocations);
        Assert.AreEqual("sessionStorage.removeItem", _jsRuntime.Invocations[0].Identifier);
        Assert.AreEqual("myKey", _jsRuntime.Invocations[0].Args[0]);
    }

    [TestMethod]
    public async Task ClearAsync_InvokesCorrectJsMethod()
    {
        // Act
        await JsInterop.ClearAsync(_jsRuntime, "localStorage", CancellationToken.None);

        // Assert
        Assert.HasCount(1, _jsRuntime.Invocations);
        Assert.AreEqual("localStorage.clear", _jsRuntime.Invocations[0].Identifier);
    }

    [TestMethod]
    public async Task LengthAsync_InvokesEvalAndReturnsCount()
    {
        // Arrange
        _jsRuntime.Results["eval"] = 5;

        // Act
        var result = await JsInterop.LengthAsync(_jsRuntime, "localStorage", CancellationToken.None);

        // Assert
        Assert.AreEqual(5, result);
        Assert.AreEqual("eval", _jsRuntime.Invocations[0].Identifier);
        Assert.AreEqual("localStorage.length", _jsRuntime.Invocations[0].Args[0]);
    }

    [TestMethod]
    public async Task KeyAsync_InvokesCorrectJsMethod()
    {
        // Arrange
        _jsRuntime.Results["localStorage.key"] = "key-at-index";

        // Act
        var result = await JsInterop.KeyAsync(_jsRuntime, "localStorage", 2, CancellationToken.None);

        // Assert
        Assert.AreEqual("key-at-index", result);
        Assert.AreEqual("localStorage.key", _jsRuntime.Invocations[0].Identifier);
        Assert.AreEqual(2, _jsRuntime.Invocations[0].Args[0]);
    }

    [TestMethod]
    public async Task SessionStorage_UsesCorrectStorageName()
    {
        // Act
        await JsInterop.SetItemAsync(_jsRuntime, "sessionStorage", "key", "val", CancellationToken.None);

        // Assert
        Assert.AreEqual("sessionStorage.setItem", _jsRuntime.Invocations[0].Identifier);
    }
}
