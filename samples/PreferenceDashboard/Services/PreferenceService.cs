using D20Tek.Blazor.BrowserStorage;
using PreferenceDashboard.Models;

namespace PreferenceDashboard.Services;

public class PreferenceService(ILocalStorageService storage)
{
    private const string StorageKey = "user-preferences";
    private readonly ILocalStorageService _storage = storage;

    public UserPreferences Current { get; private set; } = new();

    public event Action? OnChanged;

    public async Task LoadAsync()
    {
        var result = await _storage.GetAsync<UserPreferences>(StorageKey);
        if (result.IsSuccess && result.Value is not null)
        {
            Current = result.Value;
        }
    }

    public async Task SaveAsync(UserPreferences preferences)
    {
        Current = preferences;
        await _storage.SetAsync(StorageKey, preferences);
        OnChanged?.Invoke();
    }

    public async Task ClearAsync()
    {
        Current = UserPreferences.Default;
        await _storage.RemoveAsync(StorageKey);
        OnChanged?.Invoke();
    }
}
