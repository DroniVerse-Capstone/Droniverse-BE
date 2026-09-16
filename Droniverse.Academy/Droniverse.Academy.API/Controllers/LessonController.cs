using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/modules/{moduleId:guid}/lessons")]
[ApiController]
public class LessonController : ControllerBase
{
    private readonly ILogger<LessonController> _logger;
    private readonly ILessonService _lessonService;

    public LessonController(ILogger<LessonController> logger, ILessonService lessonService)
    {
        _logger = logger;
        _lessonService = lessonService;
    }

    /// <summary>
    /// Lấy danh sách bài học của mô-đun.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetLessons(Guid moduleId)
    {
        try
        {
            var lessons = await _lessonService.GetLessonsByModuleAsync(moduleId);
            return Ok(SuccessResponse<IEnumerable<LessonClientViewDTO>>.Create(lessons, "Lấy danh sách bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách bài học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết bài học.
    /// </summary>
    [HttpGet("{lessonId:guid}")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetLessonDetail(Guid moduleId, Guid lessonId)
    {
        try
        {
            var lesson = await _lessonService.GetLessonDetailAsync(moduleId, lessonId);
            return Ok(SuccessResponse<LessonClientViewDTO>.Create(lesson, "Lấy chi tiết bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết bài học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật bài học.
    /// </summary>
    /// <param name="moduleId">Mã mô-đun.</param>
    /// <param name="lessonId">Mã bài học.</param>
    /// <param name="request">Thông tin bài học cần cập nhật.</param>
    [HttpPut("{lessonId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(UpdateLessonRequestDTO), typeof(UpdateLessonRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateLesson(Guid moduleId, Guid lessonId, [FromBody] UpdateLessonRequestDTO request)
    {
        try
        {
            var updated = await _lessonService.UpdateLessonAsync(moduleId, lessonId, request);
            return Ok(SuccessResponse<LessonClientViewDTO>.Create(updated, "Cập nhật bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật bài học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa bài học.
    /// </summary>
    [HttpDelete("{lessonId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteLesson(Guid moduleId, Guid lessonId)
    {
        try
        {
            await _lessonService.DeleteLessonAsync(moduleId, lessonId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa bài học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Sắp xếp lại thứ tự bài học.
    /// </summary>
    [HttpPatch("reorder")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> ReorderLessons(Guid moduleId, [FromBody] ReorderLessonsRequestDTO request)
    {
        try
        {
            var result = await _lessonService.ReorderLessonsAsync(moduleId, request);
            return Ok(SuccessResponse<IEnumerable<LessonClientViewDTO>>.Create(result, "Sắp xếp lại bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sắp xếp lại bài học thất bại.");
            throw;
        }
    }
}
