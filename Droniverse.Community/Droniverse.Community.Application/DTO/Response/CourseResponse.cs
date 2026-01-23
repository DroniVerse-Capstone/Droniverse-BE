using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response;
public record CourseResponse(
    Guid CourseID,
    Guid CourseVersionID,
    string TitleVN,
    string TitleEN,
    string DescriptionVN,
    string DescriptionEN,
    //CourseStatus Status,
    int Version,
    string ImageUrl,
    //CourseLevel Level,
     int EstimatedDuration
    )
{
}

