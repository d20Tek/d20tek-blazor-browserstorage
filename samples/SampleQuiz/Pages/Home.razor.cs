using Microsoft.AspNetCore.Components;
using SampleQuiz.Models;
using SampleQuiz.Services;

namespace SampleQuiz.Pages;

public partial class Home
{
    private PlayerProfile? _profile;
    private string _playerName = string.Empty;
    private int _gamesPlayed;
    private int _streak;
    private int _storageCount;
    private bool _loading = true;

    [Inject] private GameService Game { get; set; } = default!;

    [Inject] private NavigationManager Nav { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _profile = await Game.GetProfileAsync();
        if (_profile is not null)
        {
            _gamesPlayed = await Game.GetGamesPlayedAsync();
            _streak = await Game.GetCurrentStreakAsync();
            _storageCount = await Game.GetLocalStorageCountAsync();
        }
        _loading = false;
    }

    private async Task CreateProfile()
    {
        if (string.IsNullOrWhiteSpace(_playerName)) return;

        _profile = new PlayerProfile
        {
            Name = _playerName.Trim(),
            CreatedDate = DateTimeOffset.UtcNow
        };

        await Game.SaveProfileAsync(_profile);
        Nav.NavigateTo("/categories");
    }

    private async Task ResetAll()
    {
        await Game.ResetAllDataAsync();
        _profile = null;
        _playerName = string.Empty;
        _gamesPlayed = 0;
        _streak = 0;
        _storageCount = 0;
    }
}
