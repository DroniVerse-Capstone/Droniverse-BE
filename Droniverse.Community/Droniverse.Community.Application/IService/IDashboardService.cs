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
        Task<AdminRevenueOverviewResponse> GetAdminRevenueOverview();
        Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(int months);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByAllClubs(int top);
        Task<AdminClubRankingResponse> GetAdminClubRankingBySpent(int top = 10);

        // Competition Stats Dashboard
        /// <summary>
        /// Thống kê cuộc thi toàn hệ thống (Admin).
        /// </summary>
        Task<CompetitionStatsResponse> GetCompetitionStats(int top = 10);

        /// <summary>
        /// Thống kê cuộc thi theo câu lạc bộ.
        /// </summary>
        Task<CompetitionStatsResponse> GetCompetitionStatsByClub(Guid clubId, int top = 10);

        // Code Stats & Top Buyers Dashboard
        /// <summary>
        /// Lấy thống kê mã code activation (phát hành/đã dùng/hết hạn) theo câu lạc bộ.
        /// </summary>
        Task<CodeStatsOverviewResponse> GetCodeStatsByClub(Guid clubId);

        /// <summary>
        /// Lấy thống kê mã code activation toàn hệ thống.
        /// </summary>
        Task<CodeStatsOverviewResponse> GetCodeStatsAdmin();

        /// <summary>
        /// Lấy danh sách top buyers theo câu lạc bộ.
        /// </summary>
        Task<TopBuyersResponse> GetTopBuyersByClub(Guid clubId, int top = 10);

        /// <summary>
        /// Lấy danh sách top buyers toàn hệ thống.
        /// </summary>
        Task<TopBuyersResponse> GetTopBuyersAdmin(int top = 10);
    }
}
