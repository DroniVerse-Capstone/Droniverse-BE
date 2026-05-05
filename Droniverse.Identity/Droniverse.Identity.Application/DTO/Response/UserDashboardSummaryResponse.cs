namespace Droniverse.Identity.Application.DTO.Response;

public record UserDashboardSummaryResponse
{
    public int TotalUser { get; init; }
    public int NewUsers { get; init; }
    public int MemberCount { get; init; }
    public int ClubOwnerCount { get; init; }
}