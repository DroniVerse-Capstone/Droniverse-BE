namespace Droniverse.Academy.Application.DTO.Request;

public class FeedbackCreateDTO
{
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public Guid CourseVersionID { get; set; }
}

