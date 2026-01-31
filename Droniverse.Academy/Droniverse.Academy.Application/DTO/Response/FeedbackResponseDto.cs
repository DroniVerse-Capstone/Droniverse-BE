namespace Droniverse.Academy.Application.DTO.Response;

public record FeedbackResponseDto(
    Guid FeedbackID,
    Guid UserID,
    int Rating,
    string Content,
    DateTime CreateAt,
    CourseVersionResponseDto CourseVersion
    )
{
    public FeedbackResponseDto() : this(default, default, default, default, default, default) { }
}

