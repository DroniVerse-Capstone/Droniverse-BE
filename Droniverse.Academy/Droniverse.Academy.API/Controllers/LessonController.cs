using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateLesson(Guid moduleId, [FromBody] CreateLessonRequestDTO request)
    {
        try
        {
            var created = await _lessonService.CreateLessonAsync(moduleId, request);
            return StatusCode(201, SuccessResponse<LessonClientViewDTO>.Create(created, "Tạo bài học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo bài học thất bại.");
            throw;
        }
    }

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

    [HttpPut("{lessonId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
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
