using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/certificates")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
/// <summary>
/// Quản lý chứng chỉ của người dùng hiện tại.
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
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<UserCertificateResponseDTO>>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCertificates(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] UserCertificateStatusFilter status = UserCertificateStatusFilter.All)
    {
        try
        {
            var result = await _service.GetMyCertificatesAsync(pageIndex, pageSize, MapUserCertificateStatus(status));
            return Ok(SuccessResponse<PaginationResult<IEnumerable<UserCertificateResponseDTO>>>.Create(
                result,
                "Lấy danh sách chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết chứng chỉ của người dùng hiện tại.
    /// </summary>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    [HttpGet("{certificateId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse<UserCertificateResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCertificate(Guid certificateId)
    {
        try
        {
            var result = await _service.GetMyCertificateAsync(certificateId);
            return Ok(SuccessResponse<UserCertificateResponseDTO>.Create(result, "Lấy chi tiết chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết chứng chỉ thất bại.");
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
