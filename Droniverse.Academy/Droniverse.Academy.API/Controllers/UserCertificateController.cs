using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy")]
[ApiController]
public class UserCertificateController : ControllerBase
{
    private readonly ILogger<UserCertificateController> _logger;
    private readonly IUserCertificateService _service;

    public UserCertificateController(ILogger<UserCertificateController> logger, IUserCertificateService service)
    {
        _logger = logger;
        _service = service;
    }

    // POST /academy/certificates/{certificateId}/users/{userId}
    [HttpPost("certificates/{certificateId:guid}/users/{userId:guid}")]
    public async Task<IActionResult> GrantCertificate(Guid certificateId, Guid userId)
    {
        try
        {
            await _service.GrantCertificateToUserAsync(certificateId, userId);
            return Ok(SuccessResponse<object>.Create(null!, "Cấp chứng chỉ cho người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cấp chứng chỉ cho người dùng thất bại.");
            throw;
        }
    }

    // GET /academy/users/{userId}/certificates
    [HttpGet("users/{userId:guid}/certificates")]
    [Authorize(Roles = Roles.AdminOrManagerRoles)]
    public async Task<IActionResult> GetUserCertificates(Guid userId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _service.GetUserCertificatesAsync(userId, pageIndex, pageSize);
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách chứng chỉ của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ của người dùng thất bại.");
            throw;
        }
    }

    // GET /academy/users/{userId}/certificates/{certificateId}
    [HttpGet("users/{userId:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = Roles.AdminOrManagerRoles)]
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

    // GET /academy/certificates/{certificateId}/users
    [HttpGet("certificates/{certificateId:guid}/users")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetUsersByCertificate(Guid certificateId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _service.GetUsersByCertificateAsync(certificateId, pageIndex, pageSize);
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách người dùng theo chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách người dùng theo chứng chỉ thất bại.");
            throw;
        }
    }

    // PATCH /academy/users/{userId}/certificates/{certificateId}/revoke
    [HttpPatch("users/{userId:guid}/certificates/{certificateId:guid}/revoke")]
    [Authorize(Roles = Roles.Admin)]
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
}
