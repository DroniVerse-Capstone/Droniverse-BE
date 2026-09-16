namespace Droniverse.Academy.Application.DTO.Response;

public class QuizLearningStateDTO
{
    public QuizClientViewDTO Quiz { get; set; } = null!;
    public QuizAttemptDTO? Attempt { get; set; }
}
