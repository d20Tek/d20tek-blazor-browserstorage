using Microsoft.AspNetCore.Components;
using SampleQuiz.Models;
using SampleQuiz.Services;

namespace SampleQuiz.Pages;

public partial class Quiz
{
    [Parameter] public string Category { get; set; } = string.Empty;

    [Inject] private GameService Game { get; set; } = default!;

    [Inject] private NavigationManager Nav { get; set; } = default!;

    [Inject] private QuestionBank Questions { get; set; } = default!;

    private List<QuizQuestion> _questions = [];
    private int _currentIndex;
    private int _selectedAnswer = -1;
    private bool _answered;
    private int _score;
    private int _streak;
    private DateTimeOffset _startTime;

    protected override async Task OnInitializedAsync()
    {
        var decoded = Uri.UnescapeDataString(Category);
        await Questions.LoadAsync();

        // Try to recover session state
        var savedState = await Game.GetQuizStateAsync();
        if (savedState is not null && savedState.Category == decoded && savedState.QuestionIndex > 0)
        {
            // Recover the same questions from saved state
            _questions = savedState.RoundQuestions ?? Questions.GetRoundQuestions(decoded);
            _currentIndex = savedState.QuestionIndex;
            _startTime = savedState.StartTime;

            // Recalculate score from saved answers
            _score = 0;
            for (var q = 0; q < savedState.SelectedAnswers.Count && q < _questions.Count; q++)
            {
                if (savedState.SelectedAnswers[q] == _questions[q].CorrectIndex)
                    _score++;
            }
        }
        else
        {
            _questions = Questions.GetRoundQuestions(decoded);
            _startTime = DateTimeOffset.UtcNow;
        }

        _streak = await Game.GetCurrentStreakAsync();
    }

    private void SelectAnswer(int index)
    {
        if (!_answered)
            _selectedAnswer = index;
    }

    private async Task ConfirmAnswer()
    {
        if (_selectedAnswer < 0 || _answered) return;

        _answered = true;

        if (_selectedAnswer == _questions[_currentIndex].CorrectIndex)
        {
            _score++;
            _streak++;
        }
        else
        {
            _streak = 0;
        }

        await Game.SaveCurrentStreakAsync(_streak);

        // Save quiz state to session storage for recovery
        var state = new QuizState
        {
            QuestionIndex = _currentIndex + 1,
            SelectedAnswers = GetAnswersSoFar(),
            Category = Uri.UnescapeDataString(Category),
            StartTime = _startTime,
            RoundQuestions = _questions
        };
        await Game.SaveQuizStateAsync(state);
    }

    private List<int> GetAnswersSoFar()
    {
        // Build a list of answers up to current question
        var answers = new List<int>();
        for (var i = 0; i <= _currentIndex; i++)
        {
            answers.Add(i == _currentIndex ? _selectedAnswer : -1);
        }
        return answers;
    }

    private void NextQuestion()
    {
        _currentIndex++;
        _selectedAnswer = -1;
        _answered = false;

        if (_currentIndex >= _questions.Count)
        {
            var duration = DateTimeOffset.UtcNow - _startTime;
            Nav.NavigateTo($"/results/{Uri.EscapeDataString(Uri.UnescapeDataString(Category))}/{_score}/{_questions.Count}/{(int)duration.TotalSeconds}");
        }
    }
}
