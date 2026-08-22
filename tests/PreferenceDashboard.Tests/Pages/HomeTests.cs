namespace PreferenceDashboard.Tests.Pages;

[TestClass]
public class HomeTests
{
    private const string BannerDismissedKey = "whats-new-dismissed";

    [TestMethod]
    public void Home_RendersBanner_WhenSessionStorageHasNoDismissedFlag()
    {
        // Arrange
        using var ctx = new BunitContext();
        ctx.AddSessionStorage();

        // Act
        var cut = ctx.Render<Home>();

        // Assert
        var banner = cut.Find(".whats-new-banner");
        Assert.Contains("What's New", banner.TextContent);
    }

    [TestMethod]
    public void Home_HidesBanner_WhenSessionStorageHasDismissedFlagSet()
    {
        // Arrange
        using var ctx = new BunitContext();
        ctx.AddSessionStorage();
        ctx.GetSessionStorage().Seed(BannerDismissedKey, true);

        // Act
        var cut = ctx.Render<Home>();

        // Assert
        Assert.IsEmpty(cut.FindAll(".whats-new-banner"));
    }

    [TestMethod]
    public void DismissBanner_HidesBannerAndPersistsFlagToSessionStorage()
    {
        // Arrange
        using var ctx = new BunitContext();
        ctx.AddSessionStorage();
        var cut = ctx.Render<Home>();

        // Act
        cut.Find(".whats-new-banner button").Click();

        // Assert
        Assert.IsEmpty(cut.FindAll(".whats-new-banner"));

        var snapshot = ctx.GetSessionStorage().Snapshot;
        Assert.IsTrue(snapshot.ContainsKey(BannerDismissedKey));
        Assert.AreEqual("true", snapshot[BannerDismissedKey]);
    }

    [TestMethod]
    public void DismissBanner_RaisesChangedEvent_OnSessionStorage()
    {
        // Arrange
        using var ctx = new BunitContext();
        ctx.AddSessionStorage();
        var session = ctx.Services.GetRequiredService<ISessionStorageService>();
        using var _ = session.RecordChanges(out var events);

        var cut = ctx.Render<Home>();

        // Act
        cut.Find(".whats-new-banner button").Click();

        // Assert
        Assert.HasCount(1, events);
        Assert.AreEqual(BannerDismissedKey, events[0].Key);
    }

    [TestMethod]
    public void DismissBanner_LogsErrorToConsole_WhenSessionStorageIsUnavailable()
    {
        // Arrange
        using var ctx = new BunitContext();
        ctx.AddSessionStorage();
        ctx.GetSessionStorage().SimulateUnavailable();

        var cut = ctx.Render<Home>();

        var originalError = Console.Error;
        using var writer = new StringWriter();
        Console.SetError(writer);

        try
        {
            // Act
            cut.Find(".whats-new-banner button").Click();

            // Assert
            Assert.AreEqual(0, cut.FindAll(".whats-new-banner").Count);
            Assert.IsFalse(ctx.GetSessionStorage().Snapshot.ContainsKey(BannerDismissedKey));
            Assert.Contains("[Home] Failed to persist banner-dismissed flag", writer.ToString());
        }
        finally
        {
            Console.SetError(originalError);
        }
    }
}
