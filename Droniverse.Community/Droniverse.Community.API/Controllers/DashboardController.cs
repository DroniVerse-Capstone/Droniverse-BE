using Microsoft.AspNetCore.Authorization;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Request;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Community.API.Controllers
{
    [ApiController]
    [Route("community/dashboards")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Lấy dữ liệu tổng quan doanh thu của 1 câu lạc bộ bất kỳ
        /// </summary>
       
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <returns>
        /// 200 OK - Trả về doanh thu của câu lạc bộ.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("revenue/clubs/{clubId:guid}/overview")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueOverviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueOverview(Guid clubId)
        {
            var data = await _dashboardService.GetRevenueOverviewByClub(clubId);
            return SuccessResponse<RevenueOverviewResponse>.Create(data, "Lấy tổng quan doanh thu câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy biểu đồ tăng doanh thu theo tháng của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="months">Số tháng cần lấy dữ liệu (mặc định 12). Bỏ qua nếu sử dụng fromDate và toDate.</param>
        /// <param name="fromDate">Ngày bắt đầu (inclusive). Format: yyyy-MM-dd. Nếu cung cấp, sẽ override tham số months.</param>
        /// <param name="toDate">Ngày kết thúc (inclusive). Format: yyyy-MM-dd. Cần cung cấp cùng với fromDate.</param>
        /// <returns>
        /// 200 OK - Trả về danh sách chi phí theo tháng hoặc ngày.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("expense/clubs/{clubId:guid}/growth")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueGrowthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueGrowth(Guid clubId, [FromQuery] int months = 12, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            RevenueGrowthResponse data;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                // Use date range filter
                data = await _dashboardService.GetRevenueGrowthByClub(clubId, fromDate.Value, toDate.Value);
            }
            else
            {
                // Use months filter (default behavior)
                data = await _dashboardService.GetRevenueGrowthByClub(clubId, months);
            }
            
            return SuccessResponse<RevenueGrowthResponse>.Create(data, "Lấy tăng trưởng doanh thu câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy chi phí theo khóa học của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="top">Số lượng khóa học có chi phí cao nhất cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về chi phí theo khóa học.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("expense/clubs/{clubId:guid}/by-course")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseRevenueResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueByCourse(Guid clubId, [FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetRevenueByCourseByClub(clubId, top);
            return SuccessResponse<ClubCourseRevenueResponse>.Create(data, "Lấy doanh thu theo khóa học của câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy dữ liệu tổng quan tài chính của tất cả các câu lạc bộ theo các giao dịch đã thanh toán thành công.
        /// </summary>
        /// <remarks>
        /// Bao gồm nhóm chỉ số doanh thu, lợi nhuận và KPI giao dịch.
        /// - Doanh thu: <b>TotalRevenue</b>, <b>RevenueThisMonth</b>, <b>RevenueLastMonth</b>, <b>RevenueGrowthRate</b>.
        /// - Lợi nhuận: <b>NetProfit</b>, <b>ProfitThisMonth</b>, <b>ProfitLastMonth</b>, <b>ProfitGrowthRate</b>.
        /// - KPI giao dịch: <b>TotalTransactions</b>, <b>TransactionsThisMonth</b>.
        /// 
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// 
        /// Trong đó:
        /// - RevenueGrowthRate = ((RevenueThisMonth - RevenueLastMonth) / RevenueLastMonth) * 100.
        /// - ProfitGrowthRate = ((ProfitThisMonth - ProfitLastMonth) / ProfitLastMonth) * 100.
        /// - Nếu mẫu số bằng 0 thì tăng trưởng trả về 100 khi giá trị tháng hiện tại &gt; 0, ngược lại trả về 0.
        /// </remarks>
        /// <returns>
        /// 200 OK - Trả về đầy đủ các chỉ số doanh thu/lợi nhuận/KPI giao dịch của hệ thống.
        /// </returns>
        [HttpGet("revenue/admin/overview")]
        [ProducesResponseType(typeof(SuccessResponse<AdminRevenueOverviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAdminRevenueOverview()
        {
            var data = await _dashboardService.GetAdminRevenueOverview();
            return SuccessResponse<AdminRevenueOverviewResponse>.Create(data, "Lấy tổng quan doanh thu admin thành công!");
        }

        /// <summary>
        /// Lấy biểu đồ tăng trưởng doanh thu theo tháng của toàn bộ hệ thống.
        /// </summary>
        /// <remarks>
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// </remarks>
        /// <param name="months">Số tháng cần lấy dữ liệu (mặc định 12). Bỏ qua nếu sử dụng fromDate và toDate.</param>
        /// <param name="fromDate">Ngày bắt đầu (inclusive). Format: yyyy-MM-dd. Nếu cung cấp, sẽ override tham số months.</param>
        /// <param name="toDate">Ngày kết thúc (inclusive). Format: yyyy-MM-dd. Cần cung cấp cùng với fromDate.</param>
        /// <returns>
        /// 200 OK - Trả về danh sách doanh thu theo tháng hoặc ngày của tất cả club.
        /// </returns>
        [HttpGet("revenue/admin/growth")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueGrowthResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAdminRevenueGrowth([FromQuery] int months = 12, [FromQuery] DateTime? fromDate = null, [FromQuery] DateTime? toDate = null)
        {
            RevenueGrowthResponse data;
            
            if (fromDate.HasValue && toDate.HasValue)
            {
                // Use date range filter
                data = await _dashboardService.GetRevenueGrowthByAllClubs(fromDate.Value, toDate.Value);
            }
            else
            {
                // Use months filter (default behavior)
                data = await _dashboardService.GetRevenueGrowthByAllClubs(months);
            }
            
            return SuccessResponse<RevenueGrowthResponse>.Create(data, "Lấy tăng trưởng doanh thu hệ thống thành công!");
        }

        /// <summary>
        /// Lấy doanh thu theo khóa học của toàn bộ hệ thống.
        /// </summary>
        /// <remarks>
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// </remarks>
        /// <param name="top">Số lượng khóa học doanh thu cao nhất cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về doanh thu theo khóa học của tất cả club.
        /// </returns>
        [HttpGet("revenue/admin/by-course")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseRevenueResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAdminRevenueByCourse([FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetRevenueByCourseByAllClubs(top);
            return SuccessResponse<ClubCourseRevenueResponse>.Create(data, "Lấy doanh thu theo khóa học của hệ thống thành công!");
        }

        /// <summary>
        /// Lấy bảng xếp hạng câu lạc bộ theo số tiền bỏ ra mua khóa học.
        /// </summary>
        /// <remarks>
        /// Trả về danh sách các câu lạc bộ được sắp xếp giảm dần theo tổng số tiền họ bỏ ra để mua khóa học (dựa vào CLUB_IMPORT).
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// </remarks>
        /// <param name="top">Số lượng club hàng đầu cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về bảng xếp hạng club theo tiền bỏ ra mua khóa học.
        /// </returns>
        [HttpGet("revenue/admin/club-ranking")]
        [ProducesResponseType(typeof(SuccessResponse<AdminClubRankingResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAdminClubRanking([FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetAdminClubRankingBySpent(top);
            return SuccessResponse<AdminClubRankingResponse>.Create(data, "Lấy bảng xếp hạng câu lạc bộ theo tiền bỏ ra thành công!");
        }

        // ===================== Competition Stats =====================

        /// <summary>
        /// Lấy thống kê cuộc thi toàn hệ thống với hỗ trợ filter.
        /// </summary>
        /// <remarks>
        /// Trả về tổng quan (tổng số, đang diễn ra, đã hoàn thành, đã hủy, nháp, tổng người tham gia, trung bình)
        /// và danh sách top cuộc thi có số người tham gia cao nhất.
        /// Hỗ trợ lọc theo: trạng thái, giai đoạn, club, ngày, người tạo, người cập nhật, số vòng, số giải, số thí sinh.
        /// </remarks>
        /// <param name="top">Số lượng cuộc thi top cần lấy (mặc định 10).</param>
        /// <param name="competitionStatus">Lọc theo trạng thái cuộc thi (nullable).</param>
        /// <param name="competitionPhase">Lọc theo giai đoạn cuộc thi (nullable).</param>
        /// <param name="clubId">Lọc theo ID câu lạc bộ (nullable).</param>
        /// <param name="startDateFrom">Lọc cuộc thi có ngày bắt đầu từ ngày này (nullable).</param>
        /// <param name="startDateTo">Lọc cuộc thi có ngày bắt đầu đến ngày này (nullable).</param>
        /// <param name="endDateFrom">Lọc cuộc thi có ngày kết thúc từ ngày này (nullable).</param>
        /// <param name="endDateTo">Lọc cuộc thi có ngày kết thúc đến ngày này (nullable).</param>
        /// <param name="createdBy">Lọc cuộc thi được tạo bởi user ID này (nullable).</param>
        /// <param name="updatedBy">Lọc cuộc thi được cập nhật bởi user ID này (nullable).</param>
        /// <param name="minTotalRounds">Lọc cuộc thi có số vòng thi tối thiểu (nullable).</param>
        /// <param name="maxTotalRounds">Lọc cuộc thi có số vòng thi tối đa (nullable).</param>
        /// <param name="minTotalPrizes">Lọc cuộc thi có số giải thưởng tối thiểu (nullable).</param>
        /// <param name="maxTotalPrizes">Lọc cuộc thi có số giải thưởng tối đa (nullable).</param>
        /// <param name="minTotalCompetitors">Lọc cuộc thi có số thí sinh tối thiểu (nullable).</param>
        /// <param name="maxTotalCompetitors">Lọc cuộc thi có số thí sinh tối đa (nullable).</param>
        [HttpGet("competitions/admin/stats")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionStatsResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetCompetitionStats(
            [FromQuery] int top = 10,
            [FromQuery] CompetitionStatus? competitionStatus = null,
            [FromQuery] CompetitionLifeCycleStatus? competitionPhase = null,
            [FromQuery] Guid? clubId = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] DateTime? endDateFrom = null,
            [FromQuery] DateTime? endDateTo = null,
            [FromQuery] Guid? createdBy = null,
            [FromQuery] Guid? updatedBy = null,
            [FromQuery] int? minTotalRounds = null,
            [FromQuery] int? maxTotalRounds = null,
            [FromQuery] int? minTotalPrizes = null,
            [FromQuery] int? maxTotalPrizes = null,
            [FromQuery] int? minTotalCompetitors = null,
            [FromQuery] int? maxTotalCompetitors = null)
        {
            var filter = new CompetitionFilterRequest
            {
                CompetitionStatus = competitionStatus,
                CompetitionPhase = competitionPhase,
                ClubId = clubId,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                EndDateFrom = endDateFrom,
                EndDateTo = endDateTo,
                CreatedBy = createdBy,
                UpdatedBy = updatedBy,
                MinTotalRounds = minTotalRounds,
                MaxTotalRounds = maxTotalRounds,
                MinTotalPrizes = minTotalPrizes,
                MaxTotalPrizes = maxTotalPrizes,
                MinTotalCompetitors = minTotalCompetitors,
                MaxTotalCompetitors = maxTotalCompetitors
            };
            var data = await _dashboardService.GetCompetitionStats(top, filter);
            return SuccessResponse<CompetitionStatsResponse>.Create(data, "Lấy thống kê cuộc thi toàn hệ thống thành công!");
        }

        /// <summary>
        /// Lấy thống kê cuộc thi theo câu lạc bộ với hỗ trợ filter.
        /// </summary>
        /// <remarks>
        /// Trả về tổng quan và top cuộc thi trong phạm vi 1 câu lạc bộ cụ thể.
        /// Hỗ trợ lọc theo: trạng thái, giai đoạn, ngày, người tạo, người cập nhật, số vòng, số giải, số thí sinh.
        /// </remarks>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="top">Số lượng cuộc thi top cần lấy (mặc định 10).</param>
        /// <param name="competitionStatus">Lọc theo trạng thái cuộc thi (nullable).</param>
        /// <param name="competitionPhase">Lọc theo giai đoạn cuộc thi (nullable).</param>
        /// <param name="startDateFrom">Lọc cuộc thi có ngày bắt đầu từ ngày này (nullable).</param>
        /// <param name="startDateTo">Lọc cuộc thi có ngày bắt đầu đến ngày này (nullable).</param>
        /// <param name="endDateFrom">Lọc cuộc thi có ngày kết thúc từ ngày này (nullable).</param>
        /// <param name="endDateTo">Lọc cuộc thi có ngày kết thúc đến ngày này (nullable).</param>
        /// <param name="createdBy">Lọc cuộc thi được tạo bởi user ID này (nullable).</param>
        /// <param name="updatedBy">Lọc cuộc thi được cập nhật bởi user ID này (nullable).</param>
        /// <param name="minTotalRounds">Lọc cuộc thi có số vòng thi tối thiểu (nullable).</param>
        /// <param name="maxTotalRounds">Lọc cuộc thi có số vòng thi tối đa (nullable).</param>
        /// <param name="minTotalPrizes">Lọc cuộc thi có số giải thưởng tối thiểu (nullable).</param>
        /// <param name="maxTotalPrizes">Lọc cuộc thi có số giải thưởng tối đa (nullable).</param>
        /// <param name="minTotalCompetitors">Lọc cuộc thi có số thí sinh tối thiểu (nullable).</param>
        /// <param name="maxTotalCompetitors">Lọc cuộc thi có số thí sinh tối đa (nullable).</param>
        [HttpGet("competitions/clubs/{clubId:guid}/stats")]
        [ProducesResponseType(typeof(SuccessResponse<CompetitionStatsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetCompetitionStatsByClub(
            Guid clubId,
            [FromQuery] int top = 10,
            [FromQuery] CompetitionStatus? competitionStatus = null,
            [FromQuery] CompetitionLifeCycleStatus? competitionPhase = null,
            [FromQuery] DateTime? startDateFrom = null,
            [FromQuery] DateTime? startDateTo = null,
            [FromQuery] DateTime? endDateFrom = null,
            [FromQuery] DateTime? endDateTo = null,
            [FromQuery] Guid? createdBy = null,
            [FromQuery] Guid? updatedBy = null,
            [FromQuery] int? minTotalRounds = null,
            [FromQuery] int? maxTotalRounds = null,
            [FromQuery] int? minTotalPrizes = null,
            [FromQuery] int? maxTotalPrizes = null,
            [FromQuery] int? minTotalCompetitors = null,
            [FromQuery] int? maxTotalCompetitors = null)
        {
            var filter = new CompetitionFilterRequest
            {
                CompetitionStatus = competitionStatus,
                CompetitionPhase = competitionPhase,
                StartDateFrom = startDateFrom,
                StartDateTo = startDateTo,
                EndDateFrom = endDateFrom,
                EndDateTo = endDateTo,
                CreatedBy = createdBy,
                UpdatedBy = updatedBy,
                MinTotalRounds = minTotalRounds,
                MaxTotalRounds = maxTotalRounds,
                MinTotalPrizes = minTotalPrizes,
                MaxTotalPrizes = maxTotalPrizes,
                MinTotalCompetitors = minTotalCompetitors,
                MaxTotalCompetitors = maxTotalCompetitors
            };
            var data = await _dashboardService.GetCompetitionStatsByClub(clubId, top, filter);
            return SuccessResponse<CompetitionStatsResponse>.Create(data, "Lấy thống kê cuộc thi của câu lạc bộ thành công!");
        }

        // ===================== Code Stats =====================

        /// <summary>
        /// Lấy thống kê mã code activation của câu lạc bộ.
        /// </summary>
        /// <remarks>
        /// Bao gồm tổng số code phát hành, số đã dùng, còn khả dụng, hết hạn, và tỉ lệ sử dụng.
        /// Dữ liệu được lấy từ Academy service.
        /// </remarks>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <returns>
        /// 200 OK - Trả về thống kê mã code của câu lạc bộ.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("codes/clubs/{clubId:guid}/stats")]
        [ProducesResponseType(typeof(SuccessResponse<CodeStatsOverviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetCodeStatsByClub(Guid clubId)
        {
            var data = await _dashboardService.GetCodeStatsByClub(clubId);
            return SuccessResponse<CodeStatsOverviewResponse>.Create(data, "Lấy thống kê mã code của câu lạc bộ thành công!");
        }

        ///// <summary>
        ///// Lấy thống kê mã code activation toàn hệ thống.
        ///// </summary>
        ///// <remarks>
        ///// Bao gồm tổng số code phát hành, số đã dùng, còn khả dụng, hết hạn, và tỉ lệ sử dụng của toàn hệ thống.
        ///// Dữ liệu được lấy từ Academy service.
        ///// </remarks>
        ///// <returns>
        ///// 200 OK - Trả về thống kê mã code toàn hệ thống.
        ///// </returns>
        //[HttpGet("codes/admin/stats")]
        //[ProducesResponseType(typeof(SuccessResponse<CodeStatsOverviewResponse>), StatusCodes.Status200OK)]
        //[Authorize(Roles = Roles.AdminOrSystemManager)]
        //public async Task<ApiResponse> GetCodeStatsAdmin()
        //{
        //    var data = await _dashboardService.GetCodeStatsAdmin();
        //    return SuccessResponse<CodeStatsOverviewResponse>.Create(data, "Lấy thống kê mã code toàn hệ thống thành công!");
        //}

        // ===================== Top Buyers =====================

        /// <summary>
        /// Lấy danh sách top buyers theo câu lạc bộ.
        /// </summary>
        /// <remarks>
        /// Trả về danh sách những người dùng mua nhiều nhất (USER_PURCHASE) trong phạm vi 1 câu lạc bộ,
        /// được sắp xếp giảm dần theo tổng số tiền bỏ ra.
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// </remarks>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="top">Số lượng top buyers cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về danh sách top buyers của câu lạc bộ.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("buyers/clubs/{clubId:guid}/top")]
        [ProducesResponseType(typeof(SuccessResponse<TopBuyersResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetTopBuyersByClub(Guid clubId, [FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetTopBuyersByClub(clubId, top);
            return SuccessResponse<TopBuyersResponse>.Create(data, "Lấy danh sách top buyers của câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy danh sách top buyers toàn hệ thống.
        /// </summary>
        /// <remarks>
        /// Trả về danh sách những người dùng mua nhiều nhất (USER_PURCHASE) của toàn hệ thống,
        /// được sắp xếp giảm dần theo tổng số tiền bỏ ra.
        /// Tất cả các chỉ số đều dựa trên các order có trạng thái <b>Status = SUCCESS</b>.
        /// </remarks>
        /// <param name="top">Số lượng top buyers cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về danh sách top buyers toàn hệ thống.
        /// </returns>
        [HttpGet("buyers/admin/top")]
        [ProducesResponseType(typeof(SuccessResponse<TopBuyersResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetTopBuyersAdmin([FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetTopBuyersAdmin(top);
            return SuccessResponse<TopBuyersResponse>.Create(data, "Lấy danh sách top buyers toàn hệ thống thành công!");
        }

        // ===================== System Operations Management =====================

        /// <summary>
        /// API Nhật ký Giao dịch Hệ thống (Orders Logs)
        /// </summary>
        [HttpGet("system/orders")]
        [ProducesResponseType(typeof(SuccessResponse<SystemTransactionLogsResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetSystemTransactionLogs([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            var data = await _dashboardService.GetSystemTransactionLogs(page, limit);
            return SuccessResponse<SystemTransactionLogsResponse>.Create(data, "Lấy nhật ký giao dịch thành công!");
        }

        /// <summary>
        /// API Tóm tắt Vận hành Hệ thống (System Operations Summary)
        /// </summary>
        [HttpGet("system/summary")]
        [ProducesResponseType(typeof(SuccessResponse<SystemOperationsSummaryResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetSystemOperationsSummary()
        {
            var data = await _dashboardService.GetSystemOperationsSummary();
            return SuccessResponse<SystemOperationsSummaryResponse>.Create(data, "Lấy tóm tắt vận hành hệ thống thành công!");
        }

        /// <summary>
        /// API Xu hướng Tăng trưởng Người dùng (User Growth Trend)
        /// </summary>
        [HttpGet("users/growth")]
        [ProducesResponseType(typeof(SuccessResponse<UserGrowthTrendResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetUserGrowthTrend([FromQuery] int months = 12)
        {
            var data = await _dashboardService.GetUserGrowthTrend(months);
            return SuccessResponse<UserGrowthTrendResponse>.Create(data, "Lấy xu hướng tăng trưởng người dùng thành công!");
        }

        /// <summary>
        /// API Hoạt động Gần đây (Recent Activity Feed)
        /// </summary>
        [HttpGet("activity/recent")]
        [ProducesResponseType(typeof(SuccessResponse<RecentActivityFeedResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetRecentActivityFeed()
        {
            var data = await _dashboardService.GetRecentActivityFeed();
            return SuccessResponse<RecentActivityFeedResponse>.Create(data, "Lấy hoạt động hệ thống gần đây thành công!");
        }
    }
}