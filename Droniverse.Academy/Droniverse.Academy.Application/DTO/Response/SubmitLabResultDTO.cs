namespace Droniverse.Academy.Application.DTO.Response;

public class SubmitLabResultDTO
{
    public Guid UserLabID { get; set; }
    public bool IsCompleted { get; set; }
    public CompleteLessonResultDTO? Completion { get; set; }
}
