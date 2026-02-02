using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response;

public record ClubResponseDto(
    Guid ClubID,
    string NameVN,
    string NameEN,
    string DescriptionVN,
    string DescriptionEN,
    Guid ClubCode,
    ClubStatus Status,
    bool IsPublic,
    int LimitParticipation,
    int LimitClubManagers,
    UserResponse Creator
    )
{
    public ClubResponseDto() : this(default, default, default, default, default, default, default, default, default, default, default)
    {
    }
}