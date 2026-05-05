using Droniverse.Community.Application.DTO.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Community.API.Examples;

public class SystemOperationsSummarySuccessResponseExample : IExamplesProvider<SuccessResponse<SystemOperationsSummaryResponse>>
{
    public SuccessResponse<SystemOperationsSummaryResponse> GetExamples()
    {
        var data = new SystemOperationsSummaryResponse
        {
            PendingClubApprovals = 2,
            TotalUsers = 1245,
            NewUsersThisMonth = 27,
            MemberCount = 980,
            ClubOwnerCount = 45,
            FilterTimeLines = new List<IdentityTimelineOptionDto>
            {
                new IdentityTimelineOptionDto("day", "Hôm nay"),
                new IdentityTimelineOptionDto("week", "Tuần này"),
                new IdentityTimelineOptionDto("last_week", "Tuần trước"),
                new IdentityTimelineOptionDto("month", "Tháng này"),
                new IdentityTimelineOptionDto("last_month", "Tháng trước"),
                new IdentityTimelineOptionDto("month:2026-03", "Tháng 3/2026"),
                new IdentityTimelineOptionDto("month:2026-04", "Tháng 4/2026"),
                new IdentityTimelineOptionDto("year", "Năm nay")
            }
        };

        return SuccessResponse<SystemOperationsSummaryResponse>.Create(data, "Lấy tóm tắt vận hành hệ thống thành công!");
    }
}
