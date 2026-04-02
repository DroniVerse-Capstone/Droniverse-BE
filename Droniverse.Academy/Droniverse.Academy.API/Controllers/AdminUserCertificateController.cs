using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/admin")]
[ApiController]
[Authorize(Roles = Roles.AdminOrManagerRoles)]
/// <summary>
/// Quản lý chứng chỉ người dùng dành cho Admin/Manager.
/// </summary>
public class AdminUserCertificateController : ControllerBase
{
    private readonly ILogger<AdminUserCertificateController> _logger;
    private readonly IAdminUserCertificateService _service;

    public AdminUserCertificateController(ILogger<AdminUserCertificateController> logger, IAdminUserCertificateService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Cấp chứng chỉ cho người dùng.
    /// </summary>
    /// <param name="request">Thông tin cấp chứng chỉ cho người dùng.</param>
    [HttpPost("user-certificates/grant")]
    [SwaggerRequestExample(typeof(GrantUserCertificateRequestDTO), typeof(GrantUserCertificateRequestExample))]
    public async Task<IActionResult> GrantCertificate([FromBody] GrantUserCertificateRequestDTO request)
    {
        try
        {
            await _service.GrantCertificateToUserAsync(request);
            return Ok(SuccessResponse<object>.Create(null!, "Cấp chứng chỉ cho người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cấp chứng chỉ cho người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách chứng chỉ của một người dùng.
    /// </summary>
    /// <param name="userId">Mã người dùng.</param>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái chứng chỉ.</param>
    [HttpGet("users/{userId:guid}/certificates")]
    public async Task<IActionResult> GetUserCertificates(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] UserCertificateStatusFilter status = UserCertificateStatusFilter.All)
    {
        try
        {
            var result = await _service.GetUserCertificatesAsync(userId, pageIndex, pageSize, MapUserCertificateStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách chứng chỉ của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết chứng chỉ của người dùng.
    /// </summary>
    /// <param name="userId">Mã người dùng.</param>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    [HttpGet("users/{userId:guid}/certificates/{certificateId:guid}")]
    public async Task<IActionResult> GetUserCertificate(Guid userId, Guid certificateId)
    {
        try
        {
            var result = await _service.GetUserCertificateAsync(userId, certificateId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết chứng chỉ của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết chứng chỉ của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách người dùng theo chứng chỉ.
    /// </summary>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái chứng chỉ.</param>
    [HttpGet("certificates/{certificateId:guid}/users")]
    public async Task<IActionResult> GetUsersByCertificate(
        Guid certificateId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] UserCertificateStatusFilter status = UserCertificateStatusFilter.All)
    {
        try
        {
            var result = await _service.GetUsersByCertificateAsync(certificateId, pageIndex, pageSize, MapUserCertificateStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách người dùng theo chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách người dùng theo chứng chỉ thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Thu hồi chứng chỉ của người dùng.
    /// </summary>
    /// <param name="userId">Mã người dùng.</param>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    [HttpPatch("users/{userId:guid}/certificates/{certificateId:guid}/revoke")]
    public async Task<IActionResult> RevokeUserCertificate(Guid userId, Guid certificateId)
    {
        try
        {
            await _service.RevokeUserCertificateAsync(userId, certificateId);
            return Ok(SuccessResponse<object>.Create(null!, "Thu hồi chứng chỉ của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Thu hồi chứng chỉ của người dùng thất bại.");
            throw;
        }
    }

    private static UserCertificateStatus? MapUserCertificateStatus(UserCertificateStatusFilter status)
    {
        return status switch
        {
            UserCertificateStatusFilter.All => null,
            UserCertificateStatusFilter.Achieved => UserCertificateStatus.ACHIEVED,
            UserCertificateStatusFilter.Revoked => UserCertificateStatus.REVOKED,
            _ => null
        };
    }
}
