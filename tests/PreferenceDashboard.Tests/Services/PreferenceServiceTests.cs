namespace PreferenceDashboard.Tests.Services;

[TestClass]
public class PreferenceServiceTests
{
    private const string StorageKey = "user-preferences";

    private static (PreferenceService Service, InMemoryLocalStorageService Storage) CreateService()
    {
        var storage = new InMemoryLocalStorageService();
        return (new PreferenceService(storage), storage);
    }

    [TestMethod]
    public void Current_ReturnsDefaults_WhenNotLoaded()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        var current = service.Current;

        // Assert
        Assert.AreEqual("light", current.Theme);
        Assert.AreEqual("#1b6ec2", current.AccentColor);
        Assert.AreEqual("'Segoe UI', sans-serif", current.FontFamily);
    }

    [TestMethod]
    public async Task LoadAsync_LeavesDefaults_WhenStorageIsEmpty()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.AreEqual("light", service.Current.Theme);
    }

    [TestMethod]
    public async Task LoadAsync_PopulatesCurrent_FromSeededStorage()
    {
        // Arrange
        var (service, storage) = CreateService();
        var seeded = new UserPreferences
        {
            Theme = "dark",
            AccentColor = "#6c63ff",
            FontFamily = "'Georgia', serif"
        };
        storage.Seed(StorageKey, seeded);

        // Act
        await service.LoadAsync();

        // Assert
        Assert.AreEqual("dark", service.Current.Theme);
        Assert.AreEqual("#6c63ff", service.Current.AccentColor);
        Assert.AreEqual("'Georgia', serif", service.Current.FontFamily);
    }

    [TestMethod]
    public async Task SaveAsync_WritesPreferencesToStorage_AndUpdatesCurrent()
    {
        // Arrange
        var (service, storage) = CreateService();
        var prefs = new UserPreferences
        {
            Theme = "dark",
            AccentColor = "#0d9488",
            FontFamily = "'Verdana', sans-serif"
        };

        // Act
        await service.SaveAsync(prefs);

        // Assert
        Assert.AreSame(prefs, service.Current);
        Assert.IsTrue(storage.Snapshot.ContainsKey(StorageKey));
    }

    [TestMethod]
    public async Task SaveAsync_RoundTrips_ViaLoadAsync()
    {
        // Arrange
        var (writer, storage) = CreateService();
        var prefs = new UserPreferences
        {
            Theme = "dark",
            AccentColor = "#db2777",
            FontFamily = "'Arial', sans-serif"
        };
        await writer.SaveAsync(prefs);

        // Act
        var reader = new PreferenceService(storage);
        await reader.LoadAsync();

        // Assert
        Assert.AreEqual("dark", reader.Current.Theme);
        Assert.AreEqual("#db2777", reader.Current.AccentColor);
        Assert.AreEqual("'Arial', sans-serif", reader.Current.FontFamily);
    }

    [TestMethod]
    public async Task SaveAsync_RaisesOnChanged()
    {
        // Arrange
        var (service, _) = CreateService();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        await service.SaveAsync(new UserPreferences { Theme = "dark" });

        // Assert
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public async Task ClearAsync_RestoresDefaults_AndRemovesStorageKey()
    {
        // Arrange
        var (service, storage) = CreateService();
        await service.SaveAsync(new UserPreferences { Theme = "dark", AccentColor = "#16a34a" });

        // Act
        await service.ClearAsync();

        // Assert
        Assert.AreEqual(UserPreferences.Default.Theme, service.Current.Theme);
        Assert.AreEqual(UserPreferences.Default.AccentColor, service.Current.AccentColor);
        Assert.IsFalse(storage.Snapshot.ContainsKey(StorageKey));
    }

    [TestMethod]
    public async Task ClearAsync_RaisesOnChanged()
    {
        // Arrange
        var (service, _) = CreateService();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        await service.ClearAsync();

        // Assert
        Assert.AreEqual(1, raised);
    }

    [TestMethod]
    public async Task SaveAsync_DoesNotThrow_WhenNoSubscribersRegistered()
    {
        // Arrange
        var (service, _) = CreateService();

        // Act + Assert (no exception)
        await service.SaveAsync(new UserPreferences());
    }

    [TestMethod]
    public async Task LoadAsync_LeavesDefaults_WhenStorageIsUnavailable()
    {
        // Arrange
        var (service, storage) = CreateService();
        storage.Seed(StorageKey, new UserPreferences { Theme = "dark" });
        storage.SimulateUnavailable();

        // Act
        await service.LoadAsync();

        // Assert
        Assert.AreEqual("light", service.Current.Theme);
    }

    [TestMethod]
    public async Task LoadAsync_LeavesDefaults_WhenStoredValueIsMalformedJson()
    {
        // Arrange
        var (service, storage) = CreateService();
        storage.Seed(StorageKey, "{ not valid json");

        // Act
        await service.LoadAsync();

        // Assert
        Assert.AreEqual("light", service.Current.Theme);
        Assert.AreEqual("#1b6ec2", service.Current.AccentColor);
    }

    [TestMethod]
    public async Task SaveAsync_UpdatesCurrentAndRaisesOnChanged_EvenWhenStorageIsUnavailable()
    {
        // Arrange
        var (service, storage) = CreateService();
        storage.SimulateUnavailable();
        var raised = 0;
        service.OnChanged += () => raised++;
        var prefs = new UserPreferences { Theme = "dark" };

        // Act
        using var capture = new ConsoleErrorCapture();
        await service.SaveAsync(prefs);

        // Assert
        Assert.AreSame(prefs, service.Current);
        Assert.AreEqual(1, raised);
        Assert.IsFalse(storage.Snapshot.ContainsKey(StorageKey));
        Assert.IsTrue(capture.Output.Contains("[PreferenceService] Failed to save preferences"));
    }

    [TestMethod]
    public async Task ClearAsync_RestoresDefaultsAndRaisesOnChanged_EvenWhenStorageIsUnavailable()
    {
        // Arrange
        var (service, storage) = CreateService();
        await service.SaveAsync(new UserPreferences { Theme = "dark" });
        storage.SimulateUnavailable();
        var raised = 0;
        service.OnChanged += () => raised++;

        // Act
        using var capture = new ConsoleErrorCapture();
        await service.ClearAsync();

        // Assert
        Assert.AreEqual(UserPreferences.Default.Theme, service.Current.Theme);
        Assert.AreEqual(1, raised);
        Assert.IsTrue(capture.Output.Contains("[PreferenceService] Failed to clear preferences"));
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
