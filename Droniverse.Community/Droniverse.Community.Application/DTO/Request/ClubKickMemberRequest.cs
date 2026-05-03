namespace Droniverse.Community.Application.DTO.Request;

public record ClubKickMemberRequest
{
    public string Reason { get; init; } = string.Empty;
}
