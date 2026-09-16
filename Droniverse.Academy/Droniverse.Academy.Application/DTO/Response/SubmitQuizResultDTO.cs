namespace Droniverse.Academy.Application.DTO.Response;

public class SubmitQuizResultDTO
{
    public Guid AttemptID { get; set; }
    public float Score { get; set; }
    public bool IsPassed { get; set; }
    public float BestScore { get; set; }
    public CompleteLessonResultDTO? Completion { get; set; }
}
