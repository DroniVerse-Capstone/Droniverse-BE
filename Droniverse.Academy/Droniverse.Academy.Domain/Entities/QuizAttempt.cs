namespace Droniverse.Academy.Domain.Entities;

public class QuizAttempt
{
    public Guid AttemptID { get; set; }
    public Quiz Quiz { get; set; }
    public Guid QuizID { get; set; }
    public Guid UserID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public float? Score { get; set; }
    public bool IsPassed { get; set; }
    public ICollection<QuizQuestionAttempt> QuizQuestionAttempts { get; set; }
}
