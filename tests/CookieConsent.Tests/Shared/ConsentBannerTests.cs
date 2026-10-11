namespace CookieConsent.Tests.Shared;

[TestClass]
public class ConsentBannerTests
{
    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext().AddCookieStorage();
        ctx.Services.AddSingleton<ConsentBannerService>();
        return ctx;
    }

    [TestMethod]
    public void ConsentBanner_Renders_WhenNoConsentCookieExists()
    {
        // Arrange
        using var ctx = CreateContext();

        // Act
        var cut = ctx.Render<ConsentBanner>();

        // Assert
        var banner = cut.Find(".consent-banner");
        Assert.Contains("We use cookies.", banner.TextContent);
    }

    [TestMethod]
    public void ConsentBanner_IsHidden_WhenConsentCookieAlreadySeeded()
    {
        // Arrange
        using var ctx = CreateContext();
        ctx.GetCookieStorage().Seed("cookie-consent", ConsentPreferences.AllowAll());

        // Act
        var cut = ctx.Render<ConsentBanner>();

        // Assert
        Assert.IsEmpty(cut.FindAll(".consent-banner"));
    }

    [TestMethod]
    public void AcceptAll_HidesBanner_AndPersistsConsentToStorage()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();

        // Act
        cut.Find("button.btn-accent").Click();

        // Assert
        Assert.IsEmpty(cut.FindAll(".consent-banner"));
        Assert.IsTrue(ctx.GetCookieStorage().Snapshot.ContainsKey("cookie-consent"));
    }

    [TestMethod]
    public void Reject_HidesBanner_AndPersistsRejectionToStorage()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();

        // Act
        cut.Find("button.btn-outline-light.btn-sm:nth-of-type(2)").Click();

        // Assert
        Assert.IsEmpty(cut.FindAll(".consent-banner"));
        var service = ctx.Services.GetRequiredService<ConsentBannerService>();
        Assert.IsFalse(service.Current!.Analytics);
        Assert.IsFalse(service.Current!.Marketing);
    }

    [TestMethod]
    public void Customize_ShowsCategoryCheckboxes()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();

        // Act
        cut.Find("button.btn-outline-light.btn-sm").Click();

        // Assert
        Assert.IsNotEmpty(cut.FindAll("#consent-analytics"));
        Assert.IsNotEmpty(cut.FindAll("#consent-marketing"));
    }

    [TestMethod]
    public void Customize_Back_ReturnsToSummaryView()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();
        cut.Find("button.btn-outline-light.btn-sm").Click();

        // Act
        cut.Find("button.btn-outline-light.btn-sm").Click();

        // Assert
        Assert.IsEmpty(cut.FindAll("#consent-analytics"));
        Assert.Contains("We use cookies.", cut.Markup);
    }

    [TestMethod]
    public void Customize_SavePreferences_PersistsSelectedCheckboxesAndHidesBanner()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();
        cut.Find("button.btn-outline-light.btn-sm").Click();

        // Act
        cut.Find("#consent-analytics").Change(true);
        cut.Find("button.btn-accent.btn-sm").Click();

        // Assert
        Assert.IsEmpty(cut.FindAll(".consent-banner"));
        var service = ctx.Services.GetRequiredService<ConsentBannerService>();
        Assert.IsTrue(service.Current!.Analytics);
        Assert.IsFalse(service.Current!.Marketing);
    }

    [TestMethod]
    public void ConsentBanner_ReappearsAfterReset_ViaOnChangedEvent()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<ConsentBanner>();
        cut.Find("button.btn-accent").Click();
        Assert.IsEmpty(cut.FindAll(".consent-banner"));

        var service = ctx.Services.GetRequiredService<ConsentBannerService>();

        // Act
        cut.InvokeAsync(() => service.ResetConsentAsync());

        // Assert
        cut.WaitForAssertion(() => Assert.IsNotEmpty(cut.FindAll(".consent-banner")));
    }
}
