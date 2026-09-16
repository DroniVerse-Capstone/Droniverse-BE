namespace Droniverse.Academy.Domain.Entities;

public class QuizQuestionAttempt
{
    public Guid AttemptAnswerID { get; set; }
    public QuizAttempt QuizAttempt { get; set; }
    public Guid AttemptID { get; set; }
    public QuizQuestion QuizQuestion { get; set; }
    public Guid QuestionID { get; set; }
    public string SelectedAnswer { get; set; }
    public bool IsCorrect { get; set; }
    public float? Score { get; set; }
}
