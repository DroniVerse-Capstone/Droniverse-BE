using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy")]
[ApiController]
public class CertificateController : ControllerBase
{
    private readonly ILogger<CertificateController> _logger;
    private readonly ICertificateService _service;

    public CertificateController(ILogger<CertificateController> logger, ICertificateService service)
    {
        _logger = logger;
        _service = service;
    }

    // POST /academy/courses/{courseId}/versions/{versionId}/certificates
    [HttpPost("courses/{courseId:guid}/versions/{versionId:guid}/certificates")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateCertificate(Guid courseId, Guid versionId, [FromBody] CreateCertificateRequestDTO request)
    {
        try
        {
            var created = await _service.CreateCertificateAsync(courseId, versionId, request);
            return CreatedAtAction(nameof(GetCertificate), new { courseId = courseId, versionId = versionId }, SuccessResponse<object>.Create(created, "Tạo chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo chứng chỉ thất bại.");
            throw;
        }
    }

    // GET /academy/courses/{courseId}/versions/{versionId}/certificates
    [HttpGet("courses/{courseId:guid}/versions/{versionId:guid}/certificates")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCertificate(Guid courseId, Guid versionId)
    {
        try
        {
            var cert = await _service.GetCertificateAsync(courseId, versionId);
            return Ok(SuccessResponse<object>.Create(cert, "Lấy chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chứng chỉ thất bại.");
            throw;
        }
    }

    // PUT /academy/courses/{courseId}/versions/{versionId}/certificates/{certificateId}
    [HttpPut("courses/{courseId:guid}/versions/{versionId:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateCertificate(Guid courseId, Guid versionId, Guid certificateId, [FromBody] UpdateCertificateRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateCertificateAsync(courseId, versionId, certificateId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật chứng chỉ thất bại.");
            throw;
        }
    }

    // DELETE /academy/courses/{courseId}/versions/{versionId}/certificates/{certificateId}
    [HttpDelete("courses/{courseId:guid}/versions/{versionId:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteCertificate(Guid courseId, Guid versionId, Guid certificateId)
    {
        try
        {
            await _service.DeleteCertificateAsync(courseId, versionId, certificateId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa chứng chỉ thất bại.");
            throw;
        }
    }

    // GET /academy/certificates/{certificateId}
    [HttpGet("certificates/{certificateId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCertificateById(Guid certificateId)
    {
        try
        {
            var cert = await _service.GetCertificateByIdAsync(certificateId);
            return Ok(SuccessResponse<object>.Create(cert, "Lấy chi tiết chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết chứng chỉ thất bại.");
            throw;
        }
    }

    // POST /academy/certificates/by-ids
    [HttpPost("certificates/by-ids")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCertificatesByIds([FromBody] GetCertificatesByIdsRequestDTO request)
    {
        try
        {
            var certs = await _service.GetCertificatesByIdsAsync(request.CertificateIds);
            return Ok(SuccessResponse<object>.Create(certs, "Lấy danh sách chứng chỉ theo id thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ theo id thất bại.");
            throw;
        }
    }
}
