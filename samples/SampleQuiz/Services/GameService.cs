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
        return result.IsSuccess ? result.Value : null;
    }

    public async Task SaveProfileAsync(PlayerProfile profile) =>
        LogIfFailure(await _local.SetAsync(ProfileKey, profile), nameof(SaveProfileAsync));

    public async Task<List<ScoreEntry>> GetHighScoresAsync()
    {
        var result = await _local.GetAsync<List<ScoreEntry>>(HighScoresKey);
        return result.IsSuccess && result.Value is not null ? result.Value : [];
    }

    public async Task SaveHighScoresAsync(List<ScoreEntry> scores) =>
        LogIfFailure(await _local.SetAsync(HighScoresKey, scores), nameof(SaveHighScoresAsync));

    public async Task<int> GetGamesPlayedAsync()
    {
        var result = await _local.GetAsync<int>(GamesPlayedKey);
        return result.IsSuccess ? result.Value : 0;
    }

    public async Task<List<string>> GetCategoriesUnlockedAsync()
    {
        var result = await _local.GetAsync<List<string>>(CategoriesUnlockedKey);
        return result.IsSuccess && result.Value is not null ? result.Value : [QuestionBank.Categories[0]];
    }

    public async Task UnlockCategoryAsync(string category)
    {
        var unlocked = await GetCategoriesUnlockedAsync();
        if (!unlocked.Contains(category))
        {
            unlocked.Add(category);
            LogIfFailure(await _local.SetAsync(CategoriesUnlockedKey, unlocked), nameof(UnlockCategoryAsync));
        }
    }

    public async Task<QuizState?> GetQuizStateAsync()
    {
        var result = await _session.GetAsync<QuizState>(QuizStateKey);
        return result.IsSuccess ? result.Value : null;
    }

    public async Task SaveQuizStateAsync(QuizState state) =>
        LogIfFailure(await _session.SetAsync(QuizStateKey, state), nameof(SaveQuizStateAsync));

    public async Task<int> GetCurrentStreakAsync()
    {
        var result = await _session.GetAsync<int>(CurrentStreakKey);
        return result.IsSuccess ? result.Value : 0;
    }

    public async Task SaveCurrentStreakAsync(int streak) =>
        LogIfFailure(await _session.SetAsync(CurrentStreakKey, streak), nameof(SaveCurrentStreakAsync));

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
        LogIfFailure(await _local.SetMultipleAsync(items), nameof(SaveResultsAsync));

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

    public async Task ClearSessionStateAsync() =>
        LogIfFailure(
            await _session.RemoveMultipleAsync([QuizStateKey, CurrentStreakKey]),
            nameof(ClearSessionStateAsync));

    public async Task<int> GetLocalStorageCountAsync() => await _local.LengthAsync();

    public async Task<IReadOnlyList<string>> GetLocalStorageKeysAsync() => await _local.GetKeysAsync();

    public async Task ResetAllDataAsync()
    {
        LogIfFailure(await _local.ClearAsync(), $"{nameof(ResetAllDataAsync)} (local)");
        LogIfFailure(await _session.ClearAsync(), $"{nameof(ResetAllDataAsync)} (session)");
    }

    private static void LogIfFailure(StorageResult result, string operation)
    {
        if (!result.IsSuccess)
        {
            Console.Error.WriteLine($"[GameService.{operation}] Storage operation failed: {result.ErrorMessage}");
        }
    }
}
