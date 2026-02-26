using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request;

public record ClubCreateDto(
    string NameVN,
    string NameEN,
    string DescriptionVN,
    string DescriptionEN,
    ClubStatus Status,
    bool IsPublic,
    int LimitParticipation,
    int LimitClubManagers,
    Guid CreatedBy,
    List<Guid> CategoryIDs
    )
{}