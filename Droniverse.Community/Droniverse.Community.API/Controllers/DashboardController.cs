using Microsoft.AspNetCore.Authorization;
using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.IService;
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
        /// Lấy dữ liệu tổng quan chi phí và KPI giao dịch của câu lạc bộ theo các giao dịch đã thanh toán thành công.
        /// </summary>
        /// <remarks>
        /// Bao gồm nhóm chỉ số chi phí và KPI giao dịch.
        /// - Chi phí: <b>TotalExpense</b>, <b>ExpenseThisMonth</b>, <b>ExpenseLastMonth</b>.
        /// - KPI giao dịch: <b>TotalTransactions</b>, <b>TransactionsThisMonth</b>.
        /// </remarks>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <returns>
        /// 200 OK - Trả về chỉ số chi phí và KPI giao dịch của câu lạc bộ.
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
        /// Lấy biểu đồ tăng trưởng chi phí theo tháng của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="months">Số tháng cần lấy dữ liệu (mặc định 12).</param>
        /// <returns>
        /// 200 OK - Trả về danh sách chi phí theo tháng.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("expense/clubs/{clubId:guid}/growth")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueGrowthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueGrowth(Guid clubId, [FromQuery] int months = 12)
        {
            var data = await _dashboardService.GetRevenueGrowthByClub(clubId, months);
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
        /// Bao gồm nhóm chỉ số doanh thu, chi phí, lợi nhuận và KPI giao dịch.
        /// - Doanh thu: <b>TotalRevenue</b>, <b>RevenueThisMonth</b>, <b>RevenueLastMonth</b>, <b>RevenueGrowthRate</b>.
        /// - Lợi nhuận: <b>NetProfit</b>, <b>ProfitThisMonth</b>, <b>ProfitLastMonth</b>, <b>ProfitGrowthRate</b>.
        /// - KPI giao dịch: <b>TotalTransactions</b>, <b>TransactionsThisMonth</b>.
        /// Trong đó:
        /// - RevenueGrowthRate = ((RevenueThisMonth - RevenueLastMonth) / RevenueLastMonth) * 100.
        /// - ProfitGrowthRate = ((ProfitThisMonth - ProfitLastMonth) / ProfitLastMonth) * 100.
        /// - Nếu mẫu số bằng 0 thì tăng trưởng trả về 100 khi giá trị tháng hiện tại &gt; 0, ngược lại trả về 0.
        /// </remarks>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <returns>
        /// 200 OK - Trả về đầy đủ các chỉ số doanh thu/lợi nhuận/KPI giao dịch của hệ thống (không có chi phí).
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
        /// <param name="months">Số tháng cần lấy dữ liệu (mặc định 12).</param>
        /// <returns>
        /// 200 OK - Trả về danh sách doanh thu theo tháng của tất cả club.
        /// </returns>
        [HttpGet("revenue/admin/growth")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueGrowthResponse>), StatusCodes.Status200OK)]
        [Authorize(Roles = Roles.AdminOrSystemManager)]
        public async Task<ApiResponse> GetAdminRevenueGrowth([FromQuery] int months = 12)
        {
            var data = await _dashboardService.GetRevenueGrowthByAllClubs(months);
            return SuccessResponse<RevenueGrowthResponse>.Create(data, "Lấy tăng trưởng doanh thu hệ thống thành công!");
        }

        /// <summary>
        /// Lấy doanh thu theo khóa học của toàn bộ hệ thống.
        /// </summary>
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
    }
}