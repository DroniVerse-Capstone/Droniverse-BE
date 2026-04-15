using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy")]
[ApiController]
public class CertificateController : ControllerBase
{
    private readonly ILogger<CertificateController> _logger;
    private readonly ICertificateService _service;
    private readonly ICertificateCreationService _certificateCreationService;

    public CertificateController(
        ILogger<CertificateController> logger,
        ICertificateService service,
        ICertificateCreationService certificateCreationService)
    {
        _logger = logger;
        _service = service;
        _certificateCreationService = certificateCreationService;
    }

    /// <summary>
    /// API để tao ra chứng chỉ
    /// </summary>
    /// <remarks>
    /// Tạo chứng chỉ từ ảnh mẫu, tự động ghi tên khóa học tiếng Việt ở giữa ảnh,
    /// upload ảnh mới lên server và trả về URL ảnh chứng chỉ vừa tạo.
    /// </remarks>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="request">Thông tin chứng chỉ cần tạo.</param>
    // POST /academy/courses/{courseId}/versions/{versionId}/certificates
    [HttpPost("courses/{courseId:guid}/versions/{versionId:guid}/certificates")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(CreateCertificateRequestDTO), typeof(CreateCertificateRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCertificate(Guid courseId, Guid versionId, [FromBody] CreateCertificateRequestDTO request)
    {
        try
        {
            var created = await _certificateCreationService.CreateCertificateWithGeneratedImageAsync(
                courseId,
                versionId,
                request,
                HttpContext.RequestAborted);

            return CreatedAtAction(
                nameof(GetCertificate),
                new { courseId, versionId },
                SuccessResponse<object>.Create(new { imageUrl = created.ImageUrl }, "Tạo chứng chỉ thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo chứng chỉ thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chứng chỉ của phiên bản khóa học.
    /// </summary>
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

    /// <summary>
    /// Cập nhật chứng chỉ.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="certificateId">Mã chứng chỉ.</param>
    /// <param name="request">Thông tin chứng chỉ cần cập nhật.</param>
    // PUT /academy/courses/{courseId}/versions/{versionId}/certificates/{certificateId}
    [HttpPut("courses/{courseId:guid}/versions/{versionId:guid}/certificates/{certificateId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(UpdateCertificateRequestDTO), typeof(UpdateCertificateRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Xóa chứng chỉ.
    /// </summary>
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

    /// <summary>
    /// Lấy chi tiết chứng chỉ theo ID.
    /// </summary>
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

    /// <summary>
    /// Lấy danh sách chứng chỉ theo danh sách ID.
    /// </summary>
    /// <param name="request">Danh sách mã chứng chỉ cần truy vấn.</param>
    // POST /academy/certificates/by-ids
    [HttpPost("certificates/by-ids")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(GetCertificatesByIdsRequestDTO), typeof(GetCertificatesByIdsRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// API nội bộ giữa các service để lấy danh sách chứng chỉ rút gọn theo danh sách ID.
    /// </summary>
    [HttpPost("certificates/bulk")]
    [HttpPost("certificates/bulk/exist")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetCertificatesBulk([FromBody] IEnumerable<Guid> certificateIds, CancellationToken cancellationToken)
    {
        try
        {
            var certs = await _service.GetCertificatesBulkAsync(certificateIds, cancellationToken);
            return Ok(certs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách chứng chỉ rút gọn thất bại.");
            throw;
        }
    }
}
