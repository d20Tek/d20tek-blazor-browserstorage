namespace SampleQuiz.Models;

public class QuizQuestion
{
    public required string Question { get; set; }

    public required string[] Options { get; set; }

    public required int CorrectIndex { get; set; }

    public required string Category { get; set; }

    public required string Difficulty { get; set; } // Easy, Medium, Hard
}
