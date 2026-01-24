namespace Droniverse.Academy.Application.DTO.Request;

public record FeedbackCreateDto(
    int Rating,
    string Content,
    Guid CourseVersionID
    )
{ }

