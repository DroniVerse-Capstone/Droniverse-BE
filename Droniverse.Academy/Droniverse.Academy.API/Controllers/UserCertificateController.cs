using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
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
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GrantCertificate failed for {CertificateId}/{UserId}", certificateId, userId);
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
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserCertificates failed for {UserId}", userId);
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
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUserCertificate failed for {UserId}/{CertificateId}", userId, certificateId);
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
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetUsersByCertificate failed for {CertificateId}", certificateId);
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
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RevokeUserCertificate failed for {UserId}/{CertificateId}", userId, certificateId);
            throw;
        }
    }
}
