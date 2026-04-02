namespace Droniverse.Academy.Application.DTO.Response;

public class UserQuizQuestionAttemptResponseDTO
{
    public Guid AttemptAnswerID { get; set; }
    public Guid AttemptID { get; set; }
    public Guid QuestionID { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public float? Score { get; set; }
}
