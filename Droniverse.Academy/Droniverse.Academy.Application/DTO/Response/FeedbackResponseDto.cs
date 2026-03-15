namespace Droniverse.Academy.Application.DTO.Response;

public class FeedbackResponseDTO
{
    public Guid FeedbackID { get; set; }
    public Guid UserID { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public CourseVersionResponseDTO? CourseVersion { get; set; }
}

