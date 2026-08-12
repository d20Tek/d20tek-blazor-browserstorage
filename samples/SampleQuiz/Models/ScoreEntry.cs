namespace SampleQuiz.Models;

public class ScoreEntry
{
    public int Score { get; set; }

    public int TotalQuestions { get; set; }

    public string Category { get; set; } = string.Empty;

    public DateTimeOffset Date { get; set; } = DateTimeOffset.UtcNow;

    public TimeSpan Duration { get; set; }
}
