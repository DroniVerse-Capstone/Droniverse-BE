namespace Droniverse.Shared.DTOs.Response;

public record CourseResponseDto(
    Guid CourseID,
    Guid CreateBy,
    DateTime CreateAt
    )
{ }

