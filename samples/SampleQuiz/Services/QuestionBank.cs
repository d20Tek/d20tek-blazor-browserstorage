using System.Net.Http.Json;
using SampleQuiz.Models;

namespace SampleQuiz.Services;

public class QuestionBank(HttpClient http)
{
    public static readonly string[] Categories = [".NET", "Azure", "Windows"];
    public const string AllCategory = "All";

    private const int RoundSize = 10;
    private List<QuizQuestion> _allQuestions = [];
    private readonly HttpClient _http = http;

    public async Task LoadAsync()
    {
        if (_allQuestions.Count == 0)
        {
            _allQuestions = await _http.GetFromJsonAsync<List<QuizQuestion>>("data/questions.json") ?? [];
        }
    }

    public List<QuizQuestion> GetQuestions(string category) =>
        category == AllCategory
            ? [.. _allQuestions]
            : [.. _allQuestions.Where(q => q.Category == category)];

    public int GetQuestionCount(string category) =>
        category == AllCategory
            ? _allQuestions.Count
            : _allQuestions.Count(q => q.Category == category);

    public List<QuizQuestion> GetRoundQuestions(string category)
    {
        var pool = GetQuestions(category);
        if (pool.Count <= RoundSize)
            return pool;

        return pool.OrderBy(_ => Random.Shared.Next()).Take(RoundSize).ToList();
    }
}
