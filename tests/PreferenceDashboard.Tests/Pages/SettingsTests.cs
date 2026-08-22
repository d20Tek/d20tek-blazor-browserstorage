using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace PreferenceDashboard.Tests.Pages;

[TestClass]
public class SettingsTests
{
    private const string StorageKey = "user-preferences";

    private static BunitContext CreateContext()
    {
        var ctx = new BunitContext().AddLocalStorage();
        ctx.Services.AddSingleton<PreferenceService>();
        return ctx;
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    public void Settings_RendersDefaults_WhenLocalStorageIsEmpty()
    {
        // Arrange
        using var ctx = CreateContext();

        // Act
        var cut = ctx.Render<Settings>();

        // Assert
        var theme = cut.Find("select.form-select");
        Assert.AreEqual(
            "light",
            theme.GetAttribute("value") ?? theme.QuerySelector("option[selected]")?.GetAttribute("value") ?? "light");
    }

    [TestMethod]
    public async Task Settings_RendersStoredPreferences_WhenLocalStorageIsSeeded()
    {
        // Arrange
        using var ctx = CreateContext();
        var stored = new UserPreferences
        {
            Theme = "dark",
            AccentColor = "#6c63ff",
            FontFamily = "'Georgia', serif"
        };
        ctx.GetLocalStorage().Seed(StorageKey, stored);

        // PreferenceService.Current is populated via LoadAsync (called by MainLayout).
        // In isolation we call it explicitly before rendering the page.
        var svc = ctx.Services.GetRequiredService<PreferenceService>();
        await svc.LoadAsync();

        // Act
        var cut = ctx.Render<Settings>();

        // Assert
        var selects = cut.FindAll("select.form-select");
        Assert.AreEqual("dark", selects[0].QuerySelector("option[selected]")!.GetAttribute("value"));
        Assert.AreEqual("'Georgia', serif", selects[1].QuerySelector("option[selected]")!.GetAttribute("value"));
    }

    [TestMethod]
    public async Task SavePreferences_PersistsCurrentValuesToLocalStorage()
    {
        // Arrange
        using var ctx = CreateContext();
        var seeded = new UserPreferences
        {
            Theme = "dark",
            AccentColor = "#6c63ff",
            FontFamily = "'Verdana', sans-serif"
        };
        ctx.GetLocalStorage().Seed(StorageKey, seeded);
        await ctx.Services.GetRequiredService<PreferenceService>().LoadAsync();

        var cut = ctx.Render<Settings>();

        // Act
        await cut.InvokeAsync(() => cut.Find("button.btn-accent").Click());

        // Assert
        var snapshot = ctx.GetLocalStorage().Snapshot;
        Assert.IsTrue(snapshot.ContainsKey(StorageKey));

        var saved = JsonSerializer.Deserialize<UserPreferences>(snapshot[StorageKey])!;
        Assert.IsNotNull(saved);
    }

    [TestMethod]
    public void SavePreferences_ShowsSavedIndicator_ThenClearsIt()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Settings>();

        // Act
        cut.Find("button.btn-accent").Click();

        // Assert
        Assert.Contains("Saved!", cut.Markup);

        // Wait for the post-delay continuation to clear the indicator.
        cut.WaitForAssertion(
            () => Assert.DoesNotContain("Saved!", cut.Markup),
            TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void ResetToDefaults_RemovesPreferencesFromLocalStorage_AndClearsIndicator()
    {
        // Arrange
        using var ctx = CreateContext();
        ctx.GetLocalStorage().Seed(StorageKey, new UserPreferences { Theme = "dark" });

        var cut = ctx.Render<Settings>();

        // Act
        cut.Find("button.btn-outline-danger").Click();

        // Assert
        Assert.IsFalse(ctx.GetLocalStorage().Snapshot.ContainsKey(StorageKey));
        Assert.Contains("Reset to defaults!", cut.Markup);

        // Wait for the post-delay continuation to clear the indicator.
        cut.WaitForAssertion(
            () => Assert.DoesNotContain("Reset to defaults!", cut.Markup),
            TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public void SavePreferences_RaisesChangedEvent_OnLocalStorage()
    {
        // Arrange
        using var ctx = CreateContext();
        var local = ctx.Services.GetRequiredService<ILocalStorageService>();
        using var _ = local.RecordChanges(out var events);

        var cut = ctx.Render<Settings>();

        // Act
        cut.Find("button.btn-accent").Click();

        // Assert
        Assert.HasCount(1, events);
        Assert.AreEqual(StorageKey, events[0].Key);
    }

    [TestMethod]
    public void AccentColorButton_Click_UpdatesSelectedSwatch()
    {
        // Arrange
        using var ctx = CreateContext();
        var cut = ctx.Render<Settings>();

        // Sanity: the default accent ("Blue" / #1b6ec2) starts selected.
        Assert.IsTrue(cut.Find("button[title=\"Blue\"]").ClassList.Contains("border-dark"));
        Assert.IsFalse(cut.Find("button[title=\"Purple\"]").ClassList.Contains("border-dark"));

        // Act — click the Purple accent swatch, exercising the @onclick lambda.
        cut.Find("button[title=\"Purple\"]").Click();

        // Assert — selection ring moved to Purple and away from Blue.
        Assert.IsTrue(cut.Find("button[title=\"Purple\"]").ClassList.Contains("border-dark"));
        Assert.IsFalse(cut.Find("button[title=\"Blue\"]").ClassList.Contains("border-dark"));

        // And a third click switches to another swatch as well.
        cut.Find("button[title=\"Green\"]").Click();
        Assert.IsTrue(cut.Find("button[title=\"Green\"]").ClassList.Contains("border-dark"));
        Assert.IsFalse(cut.Find("button[title=\"Purple\"]").ClassList.Contains("border-dark"));
    }
}
