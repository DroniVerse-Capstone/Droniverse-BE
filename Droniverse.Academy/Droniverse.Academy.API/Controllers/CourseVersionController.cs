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
            return CreatedAtAction(nameof(GetCourseVersionById), new { courseId = courseId, versionId = created.CourseVersionID }, SuccessResponse<object>.Create(created, "T?o course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCourseVersion failed");
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
            return Ok(SuccessResponse<object>.Create(result, "L?y danh sách course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCourseVersions failed for {CourseId}", courseId);
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
            return Ok(SuccessResponse<object>.Create(result, "L?y chi ti?t course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCourseVersionById failed for {CourseId}/{VersionId}", courseId, versionId);
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
            return Ok(SuccessResponse<object>.Create(updated, "C?p nh?t course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateCourseVersion failed for {CourseId}/{VersionId}", courseId, versionId);
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
            return Ok(SuccessResponse<object>.Create(null!, "Xóa course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteCourseVersion failed for {CourseId}/{VersionId}", courseId, versionId);
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
            return Ok(SuccessResponse<object>.Create(null!, "Kích ho?t course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ActivateCourseVersion failed for {CourseId}/{VersionId}", courseId, versionId);
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
            return Ok(SuccessResponse<object>.Create(null!, "Vô hi?u hóa course version thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeactivateCourseVersion failed for {CourseId}/{VersionId}", courseId, versionId);
            throw;
        }
    }
}
