using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/prerequisites")]
[ApiController]
public class PrerequisiteCourseController : ControllerBase
{
    private readonly ILogger<PrerequisiteCourseController> _logger;
    private readonly IPrerequisiteCourseService _service;

    public PrerequisiteCourseController(ILogger<PrerequisiteCourseController> logger, IPrerequisiteCourseService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Thêm hoặc thay thế danh sách khóa học tiền đề cho một khóa học.
    /// Xóa tất cả records hiện tại và chèn lại theo danh sách gửi lên.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddPrerequisiteCourse(Guid courseId, [FromBody] PrerequisiteCoursesRequestDTO request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (courseId == Guid.Empty)
                return BadRequest(SuccessResponse<object>.Create(null!, "Course không hợp lệ."));

            if (request?.PrerequisiteCourseIds == null || !request.PrerequisiteCourseIds.Any())
                return BadRequest(SuccessResponse<object>.Create(null!, "Danh sách prerequisiteCourseIds không được để trống."));

            var distinctIds = request.PrerequisiteCourseIds
                .Where(id => id != Guid.Empty)
                .Distinct()
                .ToList();

            // Prevent self reference
            distinctIds.RemoveAll(id => id == courseId);

            if (distinctIds.Count == 0)
                return BadRequest(SuccessResponse<object>.Create(null!, "Danh sách prerequisiteCourseIds không chứa id hợp lệ hoặc chỉ chứa chính khóa học."));

            var createdCount = await _service.ReplacePrerequisitesAsync(courseId, distinctIds, cancellationToken);

            return Ok(SuccessResponse<object>.Create(new { created = createdCount }, "Cập nhật prerequisite thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật prerequisite thất bại.");
            throw;
        }
    }
}
