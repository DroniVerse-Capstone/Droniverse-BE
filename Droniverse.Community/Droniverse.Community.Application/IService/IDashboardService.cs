using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.IService
{
    public interface IDashboardService
    {
        // Club Manager Dashboard
        Task<RevenueOverviewResponse> GetRevenueOverviewByClub(Guid clubId);
        Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, int months);
        Task<RevenueGrowthResponse> GetRevenueGrowthByClub(Guid clubId, DateTime fromDate, DateTime toDate);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByClub(Guid clubId, int top);

        // Admin & System Manager Dashboard
        Task<AdminRevenueOverviewResponse> GetAdminRevenueOverview();
        Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(int months);
        Task<RevenueGrowthResponse> GetRevenueGrowthByAllClubs(DateTime fromDate, DateTime toDate);
        Task<ClubCourseRevenueResponse> GetRevenueByCourseByAllClubs(int top);
        Task<AdminClubRankingResponse> GetAdminClubRankingBySpent(int top = 10);

        // Competition Stats Dashboard
        /// <summary>
        /// Thống kê cuộc thi toàn hệ thống (Admin) với hỗ trợ filter.
        /// </summary>
        Task<CompetitionStatsResponse> GetCompetitionStats(int top = 10, CompetitionFilterRequest? filter = null);

        /// <summary>
        /// Thống kê cuộc thi theo câu lạc bộ với hỗ trợ filter.
        /// </summary>
        Task<CompetitionStatsResponse> GetCompetitionStatsByClub(Guid clubId, int top = 10, CompetitionFilterRequest? filter = null);

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

        /// <summary>
        /// Lấy toàn bộ user trong hệ thống kèm tổng tiền đã dùng để mua khóa học.
        /// </summary>
        Task<PaginationResult<IEnumerable<DetailDashboardUserResponse>>> GetDetailDashboardUsers(int page = 1, int pageSize = 10);

        // System Operations Management
        Task<SystemTransactionLogsResponse> GetSystemTransactionLogs(
            int page = 1,
            int limit = 10,
            OrderStatus? status = null,
            string? productName = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            DateTime? createdAtFrom = null,
            DateTime? createdAtTo = null);
        Task<IEnumerable<IdentityTimelineOptionDto>> GetSystemFilterTimeLines();
        Task<SystemOperationsSummaryResponse> GetSystemOperationsSummary(string identityFilterTimeLine = "month");
        Task<UserGrowthTrendResponse> GetUserGrowthTrend(int months = 12);
        Task<RecentActivityFeedResponse> GetRecentActivityFeed();
    }
}
