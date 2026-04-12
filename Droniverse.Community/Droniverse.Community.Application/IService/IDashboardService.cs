using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Community.Application.IService
{
    public interface IDashboardService
    {
        Task<RevenueOverviewResponse> GetRevenueOverviewByClub(Guid clubId);
        Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, int months);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByClub(Guid clubId, int top);
    }
}
