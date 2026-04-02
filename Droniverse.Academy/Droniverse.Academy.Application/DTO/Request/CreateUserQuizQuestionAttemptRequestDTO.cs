namespace Droniverse.Academy.Application.DTO.Request;

public class CreateUserQuizQuestionAttemptRequestDTO
{
    public Guid AttemptID { get; set; }
    public Guid QuestionID { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
}
