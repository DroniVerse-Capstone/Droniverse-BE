using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IDashboardService
    {
        // Club Manager Dashboard
        Task<RevenueOverviewResponse> GetRevenueOverviewByClub(Guid clubId);
        Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, int months);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByClub(Guid clubId, int top);

        // Admin & System Manager Dashboard
        Task<RevenueOverviewResponse> GetAdminRevenueOverview();
        Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(int months);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByAllClubs(int top);
        Task<AdminClubRankingResponse> GetAdminClubRankingBySpent(int top = 10);
    }
}
