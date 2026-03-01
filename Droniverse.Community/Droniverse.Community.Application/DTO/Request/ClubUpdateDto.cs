using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request;

public record ClubUpdateDto(
    string NameVN,
    string NameEN,
    string DescriptionVN,
    string DescriptionEN,
    ClubStatus Status,
    bool IsPublic,
    int LimitParticipation,
    int LimitClubManagers,
    List<Guid> CategoryIDs
    )
{}