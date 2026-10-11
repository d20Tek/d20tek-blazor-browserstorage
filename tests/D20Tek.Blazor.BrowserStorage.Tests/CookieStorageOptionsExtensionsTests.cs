namespace D20Tek.Blazor.BrowserStorage.Tests;

[TestClass]
public class CookieStorageOptionsExtensionsTests
{
    [TestMethod]
    public void ToSameSiteToken_ReturnsNone_WhenSameSiteIsNone()
    {
        // Arrange
        var options = new CookieStorageOptions { SameSite = CookieSameSiteMode.None, Secure = true };

        // Act
        var token = options.ToSameSiteToken();

        // Assert
        Assert.AreEqual("None", token);
    }

    [TestMethod]
    public void ToSameSiteToken_ReturnsStrict_WhenSameSiteIsStrict()
    {
        // Arrange
        var options = new CookieStorageOptions { SameSite = CookieSameSiteMode.Strict };

        // Act
        var token = options.ToSameSiteToken();

        // Assert
        Assert.AreEqual("Strict", token);
    }

    [TestMethod]
    public void ToSameSiteToken_ReturnsLax_WhenSameSiteIsLax()
    {
        // Arrange
        var options = new CookieStorageOptions { SameSite = CookieSameSiteMode.Lax };

        // Act
        var token = options.ToSameSiteToken();

        // Assert
        Assert.AreEqual("Lax", token);
    }
}
