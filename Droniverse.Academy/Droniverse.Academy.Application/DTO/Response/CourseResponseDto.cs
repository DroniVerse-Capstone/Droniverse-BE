using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public record CourseResponseDTO(
    Guid CourseID,
    Guid CreateBy,
    DateTime CreateAt,
    CourseStatus Status,
    CourseVersionResponseDTO courseVersion
    )
{ }

