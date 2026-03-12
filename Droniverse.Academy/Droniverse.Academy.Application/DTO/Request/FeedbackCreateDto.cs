namespace Droniverse.Academy.Application.DTO.Request;

public record FeedbackCreateDTO(
    int Rating,
    string Content,
    Guid CourseVersionID
    )
{ }

