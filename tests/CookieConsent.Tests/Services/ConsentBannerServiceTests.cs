namespace CookieConsent.Tests.Services;

[TestClass]
public class ConsentBannerServiceTests
{
    private const string ConsentKey = "cookie-consent";

    private static (ConsentBannerService Service, InMemoryCookieStorageService Storage) CreateService()
    {
        var storage = new InMemoryCookieStorageService();
        return (new ConsentBannerService(storage), storage);
    }

    [TestMethod]
    public void Current_IsNull_WhenNotLoaded()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        var current = service.Current;

        // Assert
        Assert.IsNull(current);
        Assert.IsFalse(service.HasDecided);
    }

    [TestMethod]
    public async Task LoadAsync_LeavesCurrentNull_WhenStorageIsEmpty()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.IsNull(service.Current);
        Assert.IsFalse(service.HasDecided);
    }

    [TestMethod]
    public async Task LoadAsync_PopulatesCurrent_FromSeededStorage()
    {
        // Arrange
        var (service, storage) = CreateService();
        var seeded = new ConsentPreferences { Analytics = true, Marketing = false, DecidedAtUtc = DateTime.UtcNow };
        storage.Seed(ConsentKey, seeded);

        // Act
        await service.LoadAsync();

        // Assert
        Assert.IsTrue(service.HasDecided);
        Assert.IsTrue(service.Current!.Analytics);
        Assert.IsFalse(service.Current!.Marketing);
    }

    [TestMethod]
    public async Task AcceptAllAsync_SetsAnalyticsAndMarketingTrue_AndWritesToStorage()
    {
        // Arrange
        var (service, storage) = CreateService();

        // Act
        await service.AcceptAllAsync();

        // Assert
        Assert.IsTrue(service.HasDecided);
        Assert.IsTrue(service.Current!.Analytics);
        Assert.IsTrue(service.Current!.Marketing);
        Assert.IsTrue(storage.Snapshot.ContainsKey(ConsentKey));
    }

    [TestMethod]
    public async Task RejectNonEssentialAsync_SetsAnalyticsAndMarketingFalse()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        await service.RejectNonEssentialAsync();

        // Assert
        Assert.IsTrue(service.HasDecided);
        Assert.IsFalse(service.Current!.Analytics);
        Assert.IsFalse(service.Current!.Marketing);
    }

    [TestMethod]
    public async Task SaveCustomPreferencesAsync_PersistsRequestedCombination()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        await service.SaveCustomPreferencesAsync(analytics: true, marketing: false);

        // Assert
        Assert.IsTrue(service.Current!.Analytics);
        Assert.IsFalse(service.Current!.Marketing);
    }

    [TestMethod]
    public async Task AcceptAllAsync_RaisesOnChanged()
    {
        // Arrange
        var (service, _) = CreateService();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        await service.AcceptAllAsync();

        // Assert
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public async Task ResetConsentAsync_ClearsCurrent_AndRemovesStorageKey()
    {
        // Arrange
        var (service, storage) = CreateService();
        await service.AcceptAllAsync();

        // Act
        await service.ResetConsentAsync();

        // Assert
        Assert.IsNull(service.Current);
        Assert.IsFalse(service.HasDecided);
        Assert.IsFalse(storage.Snapshot.ContainsKey(ConsentKey));
    }

    [TestMethod]
    public async Task ResetConsentAsync_RaisesOnChanged()
    {
        // Arrange
        var (service, _) = CreateService();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        await service.ResetConsentAsync();

        // Assert
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public async Task SaveCustomPreferencesAsync_DoesNotThrow_WhenNoSubscribersRegistered()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act + Assert (no exception)
        await service.SaveCustomPreferencesAsync(analytics: true, marketing: true);
    }

    [TestMethod]
    public async Task AcceptAllAsync_UpdatesCurrentAndRaisesOnChanged_EvenWhenStorageIsUnavailable()
    {
        // Arrange
        var (service, storage) = CreateService();
        storage.SimulateUnavailable();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        using var capture = new ConsoleErrorCapture();
        await service.AcceptAllAsync();

        // Assert
        Assert.IsTrue(service.HasDecided);
        Assert.AreEqual(1, raised);
        Assert.IsFalse(storage.Snapshot.ContainsKey(ConsentKey));
        Assert.IsTrue(capture.Output.Contains("[ConsentBannerService] Failed to save consent"));
    }

    [TestMethod]
    public async Task ResetConsentAsync_ClearsCurrentAndRaisesOnChanged_EvenWhenStorageIsUnavailable()
    {
        // Arrange
        var (service, storage) = CreateService();
        await service.AcceptAllAsync();
        storage.SimulateUnavailable();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        using var capture = new ConsoleErrorCapture();
        await service.ResetConsentAsync();

        // Assert
        Assert.IsNull(service.Current);
        Assert.AreEqual(1, raised);
        Assert.IsTrue(capture.Output.Contains("[ConsentBannerService] Failed to reset consent"));
    }

    private sealed class ConsoleErrorCapture : IDisposable
    {
        private readonly System.IO.TextWriter _original;
        private readonly System.IO.StringWriter _writer;

        public ConsoleErrorCapture()
        {
            _original = Console.Error;
            _writer = new System.IO.StringWriter();
            Console.SetError(_writer);
        }

        public string Output => _writer.ToString();

        public void Dispose()
        {
            Console.SetError(_original);
            _writer.Dispose();
        }
    }
}
