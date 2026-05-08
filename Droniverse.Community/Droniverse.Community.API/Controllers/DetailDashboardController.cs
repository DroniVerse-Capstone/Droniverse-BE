using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Application.HttpClients;
using Droniverse.Community.Application.IService;
using Droniverse.Community.Application.IService.Mongo;
using Droniverse.Community.Application.DTO.Response.Mongo;
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
    private readonly IOrderService _orderService;
    private readonly ITransactionService _transactionService;

    public DetailDashboardController(IDashboardService dashboardService, IdentityMicroserviceClient client, IOrderService orderService, ITransactionService transactionService)
    {
        _dashboardService = dashboardService;
        _client = client;
        _orderService = orderService;
        _transactionService = transactionService;
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
        var data = await _dashboardService.GetDetailDashboardClubManagers(page, pageSize);
        return SuccessResponse<PaginationResult<IEnumerable<DetailDashboardClubManagerResponse>>>.Create(data, "Lấy danh sách club manager thành công!");
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng của một user (dành cho dashboard chi tiết).
    /// </summary>
    /// <param name="userId">ID user cần lấy chi tiết đơn hàng.</param>
    [HttpGet("users/{userId:guid}/orders")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<UserOrderDetailResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetUserOrderDetails(Guid userId)
    {
        var details = await _orderService.GetOrdersDetailByUserId(userId);
        return SuccessResponse<IEnumerable<UserOrderDetailResponseDto>>.Create(details, "Lấy chi tiết đơn hàng user thành công.");
    }

    /// <summary>
    /// Lấy chi tiết lịch sử giao dịch (transactions) của một user.
    /// </summary>
    /// <param name="userId">ID user cần lấy lịch sử giao dịch.</param>
    /// <returns>Danh sách giao dịch (nạp/rút tiền) của user.</returns>
    [HttpGet("users/{userId:guid}/transactions")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<TransactionResponseDto>>), StatusCodes.Status200OK)]
    public async Task<ApiResponse> GetUserTransactions(Guid userId)
    {
        var transactions = await _transactionService.GetTransactionsByUserIdAsync(userId);
        return SuccessResponse<IEnumerable<TransactionResponseDto>>.Create(transactions, "Lấy lịch sử giao dịch thành công.");
    }

}