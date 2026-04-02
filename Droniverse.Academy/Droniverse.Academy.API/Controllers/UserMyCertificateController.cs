using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/certificates")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
/// <summary>
/// Xem chứng chỉ của người dùng hiện tại.
/// </summary>
public class UserCertificateController : ControllerBase
{
    private readonly ILogger<UserCertificateController> _logger;
    private readonly IUserCertificateService _service;

    public UserCertificateController(ILogger<UserCertificateController> logger, IUserCertificateService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách chứng chỉ của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái chứng chỉ.</param>
    [HttpGet]
    public async Task<IActionResult> GetMyCertificates(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] UserCertificateStatusFilter status = UserCertificateStatusFilter.All)
    {
        try
        {
            var result = await _service.GetMyCertificatesAsync(pageIndex, pageSize, MapUserCertificateStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách chứng chỉ của tôi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ của tôi thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết chứng chỉ của người dùng hiện tại.
    /// </summary>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    [HttpGet("{certificateId:guid}")]
    public async Task<IActionResult> GetMyCertificate(Guid certificateId)
    {
        try
        {
            var result = await _service.GetMyCertificateAsync(certificateId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết chứng chỉ của tôi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết chứng chỉ của tôi thất bại.");
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
