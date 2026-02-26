using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public record ClubResponseDto(
    Guid ClubID,
    string NameVN,
    string NameEN,
    string DescriptionVN,
    string DescriptionEN,
    string ClubCode,
    ClubStatus Status,
    bool IsPublic,
    int LimitParticipation,
    int LimitClubManagers,
    int totalMembers,
    int totalCourses,   
    UserResponse Creator,
    IEnumerable<CategoryResponseDto> Categories
    )
{

}