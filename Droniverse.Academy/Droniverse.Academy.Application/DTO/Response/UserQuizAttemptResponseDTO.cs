namespace Droniverse.Academy.Application.DTO.Response;

public class UserQuizAttemptResponseDTO
{
    public Guid AttemptID { get; set; }
    public Guid QuizID { get; set; }
    public Guid UserID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? SubmitTime { get; set; }
    public float? Score { get; set; }
    public bool IsPassed { get; set; }
}
