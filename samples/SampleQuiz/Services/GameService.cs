using D20Tek.Blazor.BrowserStorage;
using SampleQuiz.Models;

namespace SampleQuiz.Services;

public class GameService(ILocalStorageService local, ISessionStorageService session)
{
    private const string ProfileKey = "player-profile";
    private const string HighScoresKey = "high-scores";
    private const string GamesPlayedKey = "games-played";
    private const string CategoriesUnlockedKey = "categories-unlocked";
    private const string QuizStateKey = "quiz-state";
    private const string CurrentStreakKey = "current-streak";

    private readonly ILocalStorageService _local = local;
    private readonly ISessionStorageService _session = session;

    public async Task<PlayerProfile?> GetProfileAsync()
    {
        var result = await _local.GetAsync<PlayerProfile>(ProfileKey);
        return result.Success ? result.Value : null;
    }

    public async Task SaveProfileAsync(PlayerProfile profile) => await _local.SetAsync(ProfileKey, profile);

    public async Task<List<ScoreEntry>> GetHighScoresAsync()
    {
        var result = await _local.GetAsync<List<ScoreEntry>>(HighScoresKey);
        return result.Success && result.Value is not null ? result.Value : [];
    }

    public async Task SaveHighScoresAsync(List<ScoreEntry> scores) => await _local.SetAsync(HighScoresKey, scores);

    public async Task<int> GetGamesPlayedAsync()
    {
        var result = await _local.GetAsync<int>(GamesPlayedKey);
        return result.Success ? result.Value : 0;
    }

    public async Task<List<string>> GetCategoriesUnlockedAsync()
    {
        var result = await _local.GetAsync<List<string>>(CategoriesUnlockedKey);
        return result.Success && result.Value is not null ? result.Value : [QuestionBank.Categories[0]];
    }

    public async Task UnlockCategoryAsync(string category)
    {
        var unlocked = await GetCategoriesUnlockedAsync();
        if (!unlocked.Contains(category))
        {
            unlocked.Add(category);
            await _local.SetAsync(CategoriesUnlockedKey, unlocked);
        }
    }

    public async Task<QuizState?> GetQuizStateAsync()
    {
        var result = await _session.GetAsync<QuizState>(QuizStateKey);
        return result.Success ? result.Value : null;
    }

    public async Task SaveQuizStateAsync(QuizState state) => await _session.SetAsync(QuizStateKey, state);

    public async Task<int> GetCurrentStreakAsync()
    {
        var result = await _session.GetAsync<int>(CurrentStreakKey);
        return result.Success ? result.Value : 0;
    }

    public async Task SaveCurrentStreakAsync(int streak) => await _session.SetAsync(CurrentStreakKey, streak);

    public async Task SaveResultsAsync(ScoreEntry entry)
    {
        var scores = await GetHighScoresAsync();
        scores.Add(entry);
        scores = scores.OrderByDescending(s => s.Score).ThenBy(s => s.Date).Take(20).ToList();

        var gamesPlayed = await GetGamesPlayedAsync();
        gamesPlayed++;

        // Bulk save using SetMultipleAsync
        var items = new List<KeyValuePair<string, object>>
        {
            new(HighScoresKey, scores),
            new(GamesPlayedKey, gamesPlayed)
        };
        await _local.SetMultipleAsync(items);

        // Unlock next category based on games played
        var categories = QuestionBank.Categories;
        if (gamesPlayed >= 2 && categories.Length > 1)
        {
            await UnlockCategoryAsync(categories[1]);
        }
        if (gamesPlayed >= 5 && categories.Length > 2)
        {
            await UnlockCategoryAsync(categories[2]);
        }
    }

    public async Task ClearSessionStateAsync() => await _session.RemoveMultipleAsync([QuizStateKey, CurrentStreakKey]);

    public async Task<int> GetLocalStorageCountAsync() => await _local.LengthAsync();

    public async Task<IReadOnlyList<string>> GetLocalStorageKeysAsync() => await _local.GetKeysAsync();

    public async Task ResetAllDataAsync()
    {
        await _local.ClearAsync();
        await _session.ClearAsync();
    }
}
