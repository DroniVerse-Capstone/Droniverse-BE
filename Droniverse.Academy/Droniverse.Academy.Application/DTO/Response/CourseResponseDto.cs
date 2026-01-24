using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public record CourseResponseDto(
    Guid CourseID,
    Guid CreateBy,
    DateTime CreateAt
    )
{ }

