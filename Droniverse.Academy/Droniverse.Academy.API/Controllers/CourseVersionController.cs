using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/versions")]
[ApiController]
public class CourseVersionController : ControllerBase
{
    private readonly ILogger<CourseVersionController> _logger;
    private readonly ICourseVersionService _service;

    public CourseVersionController(ILogger<CourseVersionController> logger, ICourseVersionService service)
    {
        _logger = logger;
        _service = service;
    }

    // POST /academy/courses/{courseId}/versions
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateCourseVersion(Guid courseId, [FromBody] CreateCourseVersionRequestDTO request)
    {
        try
        {
            var created = await _service.CreateCourseVersionAsync(courseId, request);
            return CreatedAtAction(nameof(GetCourseVersionById), new { courseId = courseId, versionId = created.CourseVersionID }, SuccessResponse<object>.Create(created, "Tạo phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo phiên bản khóa học thất bại.");
            throw;
        }
    }

    // GET /academy/courses/{courseId}/versions
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCourseVersions(Guid courseId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
    {
        try
        {
            Droniverse.Academy.Domain.Enums.CourseVersionStatus? st = null;
            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Droniverse.Academy.Domain.Enums.CourseVersionStatus>(status, true, out var parsed))
                st = parsed;

            var result = await _service.GetCourseVersionsAsync(courseId, pageIndex, pageSize, st);
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách phiên bản khóa học thất bại.");
            throw;
        }
    }

    // GET /academy/courses/{courseId}/versions/{versionId}
    [HttpGet("{versionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCourseVersionById(Guid courseId, Guid versionId)
    {
        try
        {
            var result = await _service.GetCourseVersionByIdAsync(courseId, versionId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết phiên bản khóa học thất bại.");
            throw;
        }
    }

    // PUT /academy/courses/{courseId}/versions/{versionId}
    [HttpPut("{versionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateCourseVersion(Guid courseId, Guid versionId, [FromBody] UpdateCourseVersionRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateCourseVersionAsync(courseId, versionId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật phiên bản khóa học thất bại.");
            throw;
        }
    }

    // DELETE /academy/courses/{courseId}/versions/{versionId}
    [HttpDelete("{versionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteCourseVersion(Guid courseId, Guid versionId)
    {
        try
        {
            await _service.DeleteCourseVersionAsync(courseId, versionId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa phiên bản khóa học thất bại.");
            throw;
        }
    }

    // POST activate
    [HttpPost("{versionId:guid}/activate")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> Activate(Guid courseId, Guid versionId)
    {
        try
        {
            await _service.ActivateCourseVersionAsync(courseId, versionId);
            return Ok(SuccessResponse<object>.Create(null!, "Kích hoạt phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kích hoạt phiên bản khóa học thất bại.");
            throw;
        }
    }

    // POST deactivate
    [HttpPost("{versionId:guid}/deactivate")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> Deactivate(Guid courseId, Guid versionId)
    {
        try
        {
            await _service.DeactivateCourseVersionAsync(courseId, versionId);
            return Ok(SuccessResponse<object>.Create(null!, "Vô hiệu hóa phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Vô hiệu hóa phiên bản khóa học thất bại.");
            throw;
        }
    }
}
