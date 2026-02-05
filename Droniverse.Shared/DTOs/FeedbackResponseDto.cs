namespace Droniverse.Shared.DTOs.Response;

public record FeedbackResponseDto(
    Guid FeedbackID,
    Guid UserID,
    int Rating,
    string Content,
    DateTime CreateAt,
    CourseVersionResponseDto CourseVersion
    )
{
    public FeedbackResponseDto() : this(Guid.Empty, Guid.Empty, 0, string.Empty, DateTime.MinValue, null!) { }
}

