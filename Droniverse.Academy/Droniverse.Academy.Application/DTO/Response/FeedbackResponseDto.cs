namespace Droniverse.Academy.Application.DTO.Response;

public record FeedbackResponseDTO(
    Guid FeedbackID,
    Guid UserID,
    int Rating,
    string Content,
    DateTime CreateAt,
    CourseVersionResponseDTO CourseVersion
    )
{
    public FeedbackResponseDTO() : this(default, default, default, default, default, default) { }
}

