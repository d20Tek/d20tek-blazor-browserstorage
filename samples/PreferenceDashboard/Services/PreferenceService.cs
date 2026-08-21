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
        var result = await _storage.SetAsync(StorageKey, preferences);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"[PreferenceService] Failed to save preferences: {result.ErrorMessage}");
        }
        OnChanged?.Invoke();
    }

    public async Task ClearAsync()
    {
        Current = UserPreferences.Default;
        var result = await _storage.RemoveAsync(StorageKey);
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"[PreferenceService] Failed to clear preferences: {result.ErrorMessage}");
        }
        OnChanged?.Invoke();
    }
}
