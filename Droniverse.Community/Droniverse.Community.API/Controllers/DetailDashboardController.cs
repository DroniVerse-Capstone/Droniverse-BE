using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.DTO.Response.Mongo;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
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
    private readonly IDetailDashboardService _detailDashboardService;

    public DetailDashboardController(IDetailDashboardService detailDashboardService)
    {
        _detailDashboardService = detailDashboardService;
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
        var data = await _detailDashboardService.GetClubMembersWithCourseSpend(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<DetailDashboardUserResponse>>>.Create(data, "Lấy danh sách user chi tiêu khóa học thành công!");
    }

    /// <summary>
    /// Lấy toàn bộ club manager trong hệ thống kèm số tiền trong ví.
    /// </summary>
    /// <remarks>
    /// - Trả về danh sách club manager.
    /// - <b>WalletBalance</b> là số dư hiện tại trong ví của họ.
    /// - Sắp xếp giảm dần theo số dư ví.
    /// </remarks>
    /// <param name="page">Trang hiện tại, mặc định 1.</param>
    /// <param name="pageSize">Số bản ghi mỗi trang, mặc định 10.</param>
    /// <returns>
    /// 200 OK - Trả về danh sách club manager cùng số dư ví.
    /// </returns>
    [HttpGet("club-managers")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetClubManagersWithWalletBalance([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _detailDashboardService.GetClubManagersWithWalletBalance(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>>.Create(data, "Lấy danh sách club manager thành công!");
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng của một user (dành cho dashboard chi tiết).
    /// </summary>
    /// <param name="userId">ID user cần lấy chi tiết đơn hàng.</param>
    [HttpGet("users/{userId:guid}/orders")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<UserOrderDetailResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetUserOrderDetails(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var details = await _detailDashboardService.GetUserOrderDetails(userId, page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<UserOrderDetailResponseDto>>>.Create(details, "Lấy chi tiết đơn hàng user thành công.");
    }

    /// <summary>
    /// Lấy chi tiết lịch sử giao dịch (transactions) của một user.
    /// </summary>
    /// <param name="userId">ID user cần lấy lịch sử giao dịch.</param>
    /// <returns>Danh sách giao dịch (nạp/rút tiền) của user.</returns>
    [HttpGet("users/{userId:guid}/transactions")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<TransactionResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetUserTransactions(Guid userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var transactions = await _detailDashboardService.GetUserTransactions(userId, page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<TransactionResponseDto>>>.Create(transactions, "Lấy lịch sử giao dịch thành công.");
    }

    /// <summary>
    /// Lấy thống kê doanh thu khóa học kèm số người học và đánh giá trung bình.
    /// </summary>
    /// <remarks>
    /// - Kết hợp dữ liệu từ Academy Service (thông tin khóa học) và Order DB (doanh thu).
    /// - <b>TotalRevenue</b>: Tổng doanh thu (đã trừ hoàn tiền) từ các đơn hàng khóa học.
    /// - <b>TotalLearners</b>: Tổng số học viên (không tính học viên hủy/hoàn).
    /// - <b>AverageRating</b>: Điểm đánh giá trung bình của khóa học.
    /// - <b>ImageUrl</b>: Ảnh đại diện của khóa học.
    /// </remarks>
    /// <returns>
    /// 200 OK - Trả về danh sách khóa học với thống kê doanh thu.
    /// </returns>
    [HttpGet("courses")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CourseDetailDashboardResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetCourseRevenueDashboard([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _detailDashboardService.GetCourseRevenueDashboard(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<CourseDetailDashboardResponseDto>>>.Create(data, "Lấy thống kê khóa học kèm doanh thu thành công.");
    }

    /// <summary>
    /// Lấy chi tiết doanh thu và số lượng học viên của một khóa học theo từng câu lạc bộ.
    /// </summary>
    /// <param name="courseId">ID của khóa học.</param>
    /// <returns>Danh sách các câu lạc bộ có mua khóa học, sắp xếp theo doanh thu giảm dần.</returns>
    [HttpGet("courses/{courseId:guid}/clubs")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetCourseRevenueByClub(Guid courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _detailDashboardService.GetCourseRevenueByClub(courseId, page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<CourseRevenueByClubResponseDto>>>.Create(data, "Lấy chi tiết doanh thu khóa học theo club thành công.");
    }

    /// <summary>
    /// Lấy danh sách tổng quan tất cả các câu lạc bộ (Tên, thành viên, doanh thu, trạng thái).
    /// </summary>
    [HttpGet("clubs")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<ClubDashboardResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetClubDashboardAsync([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _detailDashboardService.GetClubDashboardAsync(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<ClubDashboardResponseDto>>>.Create(data, "Lấy thống kê câu lạc bộ thành công.");
    }

    /// <summary>
    /// Lấy danh sách giao dịch mua khóa học của các thành viên trong một câu lạc bộ.
    /// </summary>
    [HttpGet("clubs/{clubId:guid}/transactions")]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetClubMemberTransactionsAsync(Guid clubId, [FromQuery] Guid? courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var data = await _detailDashboardService.GetClubMemberTransactionsAsync(clubId, courseId, page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<ClubMemberTransactionResponseDto>>>.Create(data, "Lấy danh sách giao dịch câu lạc bộ thành công.");
    }
}