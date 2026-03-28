using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public class FeedbackClientViewDTO
{
    public Guid FeedbackID { get; set; }
    public Guid UserID { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreateAt { get; set; }
    public FeedbackCourseVersionClientViewDTO? CourseVersion { get; set; }
}

public class FeedbackCourseVersionClientViewDTO
{
    public Guid CourseVersionID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string? DescriptionVN { get; set; }
    public string? DescriptionEN { get; set; }
    public CourseLevel Level { get; set; }
    public int Version { get; set; }
}
