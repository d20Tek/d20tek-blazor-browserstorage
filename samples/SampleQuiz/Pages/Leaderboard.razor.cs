using Microsoft.AspNetCore.Components;
using SampleQuiz.Models;
using SampleQuiz.Services;

namespace SampleQuiz.Pages;

public partial class Leaderboard
{
    private List<ScoreEntry> _scores = [];
    private int _keyCount;
    private IReadOnlyList<string> _keys = [];
    private bool _loading = true;

    [Inject] private GameService Game { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _scores = await Game.GetHighScoresAsync();
        _keyCount = await Game.GetLocalStorageCountAsync();
        _keys = await Game.GetLocalStorageKeysAsync();
        _loading = false;
    }
}
