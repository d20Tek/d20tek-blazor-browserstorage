namespace SampleQuiz.Models;

public class QuizState
{
    public int QuestionIndex { get; set; }

    public List<int> SelectedAnswers { get; set; } = [];

    public string Category { get; set; } = string.Empty;

    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

    public List<QuizQuestion>? RoundQuestions { get; set; }
}
