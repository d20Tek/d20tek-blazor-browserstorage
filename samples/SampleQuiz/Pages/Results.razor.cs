using Microsoft.AspNetCore.Components;
using SampleQuiz.Models;
using SampleQuiz.Services;

namespace SampleQuiz.Pages;

public partial class Results
{
    [Parameter] public string Category { get; set; } = string.Empty;

    [Parameter] public int Score { get; set; }

    [Parameter] public int Total { get; set; }

    [Parameter] public int DurationSeconds { get; set; }

    [Inject] private GameService Game { get; set; } = default!;

    private bool _saved;

    protected override async Task OnInitializedAsync()
    {
        var entry = new ScoreEntry
        {
            Score = Score,
            TotalQuestions = Total,
            Category = Uri.UnescapeDataString(Category),
            Date = DateTimeOffset.UtcNow,
            Duration = TimeSpan.FromSeconds(DurationSeconds)
        };

        // Bulk save results (demonstrates SetMultipleAsync)
        await Game.SaveResultsAsync(entry);

        // Clear session state (demonstrates RemoveMultipleAsync)
        await Game.ClearSessionStateAsync();

        _saved = true;
    }
}
