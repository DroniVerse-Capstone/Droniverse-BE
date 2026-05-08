using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// API chi tiết dashboard cho dữ liệu user và chi tiêu khóa học.
/// </summary>
namespace Droniverse.Community.API.Controllers;

[ApiController]
[Route("community/detail-dashboards")]
[Authorize(Roles = Roles.AdminOrSystemManager)]
public class DetailDashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly IdentityMicroserviceClient _client;

    public DetailDashboardController(IDashboardService dashboardService, IdentityMicroserviceClient client)
    {
        _dashboardService = dashboardService;
        _client = client;
    }

    /// <summary>
    /// Lấy toàn bộ user trong hệ thống kèm avatar và tổng tiền đã dùng để mua khóa học.
    /// </summary>
    /// <remarks>
    /// - Trả về danh sách user ở dạng mini user.
    /// - Bao gồm <b>AvatarUrl</b> từ Identity service.
    /// - <b>TotalSpent</b> là tổng tiền các giao dịch mua khóa học thành công của từng user.
    /// - Các user chưa từng mua khóa học vẫn được trả về với <b>TotalSpent = 0</b>.
    /// </remarks>
    /// <param name="page">Trang hiện tại, mặc định 1.</param>
    /// <param name="pageSize">Số bản ghi mỗi trang, mặc định 10.</param>
    /// <returns>
    /// 200 OK - Trả về danh sách user cùng tổng tiền đã dùng để mua khóa học theo phân trang.
    /// </returns>
    [HttpGet("users")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<DetailDashboardUserResponse>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetClubMembersWithCourseSpend([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _dashboardService.GetDetailDashboardUsers(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<DetailDashboardUserResponse>>>.Create(data, "Lấy danh sách user chi tiêu khóa học thành công!");
    }

}