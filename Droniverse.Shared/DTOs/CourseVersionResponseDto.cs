using Droniverse.Shared.Enums.Response;

namespace Droniverse.Shared.DTOs.Response;
public record CourseVersionResponseDto(
    Guid CourseVersionID,
    string TitleVN,
    string TitleEN,
    string DescriptionVN,
    string DescriptionEN,
    CourseStatus Status,
    int Version,
    string ImageUrl,
    int EstimatedDuration,
    Guid UpdateBy,
    DateTime UpdateAt,
    CourseResponseDto Course
    )
{}

