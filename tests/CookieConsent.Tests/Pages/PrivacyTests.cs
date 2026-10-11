namespace CookieConsent.Tests.Pages;

[TestClass]
public class PrivacyTests
{
    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext().AddCookieStorage();
        ctx.Services.AddSingleton<ConsentBannerService>();
        return ctx;
    }

    [TestMethod]
    public void Privacy_RendersUncheckedCategories_WhenNoConsentCookieExists()
    {
        // Arrange
        using var ctx = CreateContext();

        // Act
        var cut = ctx.Render<Privacy>();

        // Assert
        var analytics = cut.Find("#privacy-analytics");
        var marketing = cut.Find("#privacy-marketing");
        Assert.IsFalse(analytics.HasAttribute("checked"));
        Assert.IsFalse(marketing.HasAttribute("checked"));
    }

    [TestMethod]
    public void Privacy_RendersStoredCategories_WhenConsentCookieIsSeeded()
    {
        // Arrange
        using var ctx = CreateContext();
        ctx.GetCookieStorage().Seed(
            "cookie-consent",
            new ConsentPreferences { Analytics = true, Marketing = false, DecidedAtUtc = DateTime.UtcNow });

        // Act
        var cut = ctx.Render<Privacy>();

        // Assert
        var analytics = cut.Find("#privacy-analytics");
        var marketing = cut.Find("#privacy-marketing");
        Assert.IsTrue(analytics.HasAttribute("checked"));
        Assert.IsFalse(marketing.HasAttribute("checked"));
    }

    [TestMethod]
    public void SavePreferences_PersistsSelectedCategories_AndShowsSavedIndicator()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Privacy>();

        // Act
        cut.Find("#privacy-analytics").Change(true);
        cut.Find("#privacy-marketing").Change(true);
        cut.Find("button.btn-accent").Click();

        // Assert
        Assert.Contains("Saved!", cut.Markup);
        var service = ctx.Services.GetRequiredService<ConsentBannerService>();
        Assert.IsTrue(service.Current!.Analytics);
        Assert.IsTrue(service.Current!.Marketing);
    }

    [TestMethod]
    public void SavePreferences_ClearsSavedIndicator_AfterDelay()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Privacy>();

        // Act
        cut.Find("button.btn-accent").Click();

        // Assert
        Assert.Contains("Saved!", cut.Markup);
        cut.WaitForAssertion(() => Assert.DoesNotContain("Saved!", cut.Markup), TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void ResetConsent_RemovesConsentCookie_AndShowsResetIndicator()
    {
        // Arrange
        using var ctx = CreateContext();
        ctx.GetCookieStorage().Seed(
            "cookie-consent",
            new ConsentPreferences { Analytics = true, Marketing = true, DecidedAtUtc = DateTime.UtcNow });
        var cut = ctx.Render<Privacy>();

        // Act
        cut.Find("button.btn-outline-danger").Click();

        // Assert
        Assert.Contains("Consent reset", cut.Markup);
        Assert.IsFalse(ctx.GetCookieStorage().Snapshot.ContainsKey("cookie-consent"));
        var analytics = cut.Find("#privacy-analytics");
        Assert.IsFalse(analytics.HasAttribute("checked"));
    }

    [TestMethod]
    public void ResetConsent_ClearsResetIndicator_AfterDelay()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Privacy>();

        // Act
        cut.Find("button.btn-outline-danger").Click();

        // Assert
        Assert.Contains("Consent reset", cut.Markup);
        cut.WaitForAssertion(() => Assert.DoesNotContain("Consent reset", cut.Markup), TimeSpan.FromSeconds(1));
    }
}
