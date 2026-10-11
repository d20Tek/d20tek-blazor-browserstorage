namespace CookieConsent.Tests.Pages;

[TestClass]
public class HomeTests
{
    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext().AddCookieStorage();
        ctx.Services.AddSingleton<ConsentBannerService>();
        return ctx;
    }

    [TestMethod]
    public void Home_ShowsNoDecisionMessage_WhenConsentCookieIsEmpty()
    {
        // Arrange
        using var ctx = CreateContext();

        // Act
        var cut = ctx.Render<Home>();

        // Assert
        Assert.Contains("No consent decision has been made yet.", cut.Markup);
    }

    [TestMethod]
    public void Home_ShowsStoredConsentState_WhenConsentCookieIsSeeded()
    {
        // Arrange
        using var ctx = CreateContext();
        ctx.GetCookieStorage().Seed(
            "cookie-consent",
            new ConsentPreferences { Analytics = true, Marketing = false, DecidedAtUtc = DateTime.UtcNow });

        // Act
        var cut = ctx.Render<Home>();

        // Assert
        Assert.Contains("Analytics:", cut.Markup);
        Assert.Contains("True", cut.Markup);
        Assert.DoesNotContain("No consent decision has been made yet.", cut.Markup);
    }

    [TestMethod]
    public void Home_UpdatesLiveState_WhenConsentChangesViaOnChanged()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Home>();
        var service = ctx.Services.GetRequiredService<ConsentBannerService>();

        // Act
        cut.InvokeAsync(() => service.AcceptAllAsync());

        // Assert
        cut.WaitForAssertion(() => Assert.DoesNotContain("No consent decision has been made yet.", cut.Markup));
    }
}
