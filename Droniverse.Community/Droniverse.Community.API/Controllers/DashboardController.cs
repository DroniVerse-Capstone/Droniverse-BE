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
        /// Lấy tổng quan doanh thu của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <returns>
        /// 200 OK - Trả về tổng doanh thu, doanh thu tháng này, tháng trước và tốc độ tăng trưởng.
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
        /// Lấy biểu đồ tăng trưởng doanh thu theo tháng của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="months">Số tháng cần lấy dữ liệu (mặc định 12).</param>
        /// <returns>
        /// 200 OK - Trả về danh sách doanh thu theo tháng.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("revenue/clubs/{clubId:guid}/growth")]
        [ProducesResponseType(typeof(SuccessResponse<RevenueGrowthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueGrowth(Guid clubId, [FromQuery] int months = 12)
        {
            var data = await _dashboardService.GetRevenueGrowthByClub(clubId, months);
            return SuccessResponse<RevenueGrowthResponse>.Create(data, "Lấy tăng trưởng doanh thu câu lạc bộ thành công!");
        }

        /// <summary>
        /// Lấy doanh thu theo khóa học của câu lạc bộ.
        /// </summary>
        /// <param name="clubId">ID câu lạc bộ.</param>
        /// <param name="top">Số lượng khóa học doanh thu cao nhất cần lấy (mặc định 10).</param>
        /// <returns>
        /// 200 OK - Trả về doanh thu theo khóa học.
        /// 404 NotFound - Không tìm thấy câu lạc bộ.
        /// </returns>
        [HttpGet("revenue/clubs/{clubId:guid}/by-course")]
        [ProducesResponseType(typeof(SuccessResponse<ClubCourseRevenueResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Roles = Roles.SystemRoles)]
        public async Task<ApiResponse> GetRevenueByCourse(Guid clubId, [FromQuery] int top = 10)
        {
            var data = await _dashboardService.GetRevenueByCourseByClub(clubId, top);
            return SuccessResponse<ClubCourseRevenueResponse>.Create(data, "Lấy doanh thu theo khóa học của câu lạc bộ thành công!");
        }
    }
}
