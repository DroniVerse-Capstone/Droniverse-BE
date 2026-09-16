namespace Droniverse.Community.Application.DTO.Response
{
    public class SystemOperationsSummaryResponse
    {
        public int PendingClubApprovals { get; set; }

        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }

        public int MemberCount { get; set; }
        public int ClubOwnerCount { get; set; }

        // Swagger-only example: available timeline options shown in Community API docs
        public IEnumerable<IdentityTimelineOptionDto>? FilterTimeLines { get; set; }

    }
}
