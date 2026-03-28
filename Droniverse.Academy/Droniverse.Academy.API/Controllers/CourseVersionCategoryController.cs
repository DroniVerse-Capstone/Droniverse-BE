using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/versions/{versionId:guid}/categories")]
[ApiController]
public class CourseVersionCategoryController : ControllerBase
{
    private readonly ILogger<CourseVersionCategoryController> _logger;
    private readonly ICourseVersionCategoryService _service;

    public CourseVersionCategoryController(ILogger<CourseVersionCategoryController> logger, ICourseVersionCategoryService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Gán danh mục cho phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="request">Thông tin danh mục cần gán.</param>
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(AssignCategoryRequestDTO), typeof(AssignCategoryRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddCategory(Guid courseId, Guid versionId, [FromBody] AssignCategoryRequestDTO request)
    {
        try
        {
            await _service.AddCategoryAsync(courseId, versionId, request);
            return Ok(SuccessResponse<object>.Create(null!, "Gán danh mục cho phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gán danh mục cho phiên bản khóa học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Gỡ danh mục khỏi phiên bản khóa học.
    /// </summary>
    [HttpDelete("{categoryId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> RemoveCategory(Guid courseId, Guid versionId, Guid categoryId)
    {
        try
        {
            await _service.RemoveCategoryAsync(courseId, versionId, categoryId);
            return Ok(SuccessResponse<object>.Create(null!, "Gỡ danh mục khỏi phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gỡ danh mục khỏi phiên bản khóa học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách danh mục của phiên bản khóa học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetCategories(Guid courseId, Guid versionId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _service.GetCategoriesAsync(courseId, versionId, pageIndex, pageSize);
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách danh mục của phiên bản khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách danh mục của phiên bản khóa học thất bại.");
            throw;
        }
    }
}
