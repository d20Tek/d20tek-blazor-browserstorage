using Microsoft.AspNetCore.Components;
using PreferenceDashboard.Models;
using PreferenceDashboard.Services;

namespace PreferenceDashboard.Pages;

public partial class Settings
{
    private string _theme = "light";
    private string _accentColor = "#1b6ec2";
    private string _fontFamily = "'Segoe UI', sans-serif";
    private bool _saved;
    private bool _reset;

    private readonly (string Name, string Value)[] _accentColors =
    [
        ("Blue", "#1b6ec2"),
        ("Purple", "#6c63ff"),
        ("Teal", "#0d9488"),
        ("Orange", "#ea580c"),
        ("Pink", "#db2777"),
        ("Green", "#16a34a")
    ];

    [Inject]
    private PreferenceService PreferenceService { get; set; } = default!;

    protected override void OnInitialized()
    {
        _theme = PreferenceService.Current.Theme;
        _accentColor = PreferenceService.Current.AccentColor;
        _fontFamily = PreferenceService.Current.FontFamily;
    }

    private async Task ResetToDefaults()
    {
        await PreferenceService.ClearAsync();

        _reset = true;
        await Task.Delay(200);
        _reset = false;
        StateHasChanged();
    }

    private async Task SavePreferences()
    {
        var prefs = new UserPreferences
        {
            Theme = _theme,
            AccentColor = _accentColor,
            FontFamily = _fontFamily
        };

        await PreferenceService.SaveAsync(prefs);
        _saved = true;
        await Task.Delay(200);
        _saved = false;

        StateHasChanged();
    }
}
