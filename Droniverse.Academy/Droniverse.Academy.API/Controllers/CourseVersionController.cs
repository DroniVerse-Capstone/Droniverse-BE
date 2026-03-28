using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Filters;

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

    /// <summary>
    /// Tạo phiên bản mới cho khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="request">Thông tin phiên bản cần tạo.</param>
    /// <remarks>Level gồm : EASY | MEDIUM | HARD</remarks>
    // POST /academy/courses/{courseId}/versions
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(CreateCourseVersionRequestDTO), typeof(CreateCourseVersionRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Lấy danh sách phiên bản của khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái phiên bản (dropdown enum trong Swagger). Chọn <c>All</c> để lấy toàn bộ.</param>
    // GET /academy/courses/{courseId}/versions
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCourseVersions(
        Guid courseId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] CourseVersionStatusFilter status = CourseVersionStatusFilter.All)
    {
        try
        {
            CourseVersionStatus? st = status switch
            {
                CourseVersionStatusFilter.All => null,
                CourseVersionStatusFilter.Draft => CourseVersionStatus.DRAFT,
                CourseVersionStatusFilter.Active => CourseVersionStatus.ACTIVE,
                CourseVersionStatusFilter.Deprecated => CourseVersionStatus.DEPRECATED,
                CourseVersionStatusFilter.Inactive => CourseVersionStatus.INACTIVE,
                _ => null
            };

            var result = await _service.GetCourseVersionsAsync(courseId, pageIndex, pageSize, st);
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách phiên bản khóa học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết một phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
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

    /// <summary>
    /// Cập nhật nội dung phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="request">Dữ liệu cập nhật.</param>
    /// <remarks>Level gồm : EASY | MEDIUM | HARD</remarks>
    // PUT /academy/courses/{courseId}/versions/{versionId}
    [HttpPut("{versionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(UpdateCourseVersionRequestDTO), typeof(UpdateCourseVersionRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Xóa một phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
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

    /// <summary>
    /// Kích hoạt phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    // POST activate
    [HttpPost("{versionId:guid}/activate")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Vô hiệu hóa phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    // POST deactivate
    [HttpPost("{versionId:guid}/deactivate")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
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
