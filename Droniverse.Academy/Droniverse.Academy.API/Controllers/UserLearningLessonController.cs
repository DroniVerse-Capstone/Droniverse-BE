using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/lessons")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLearningLessonController : ControllerBase
{
    private readonly ILogger<UserLearningLessonController> _logger;
    private readonly ILearningService _service;

    public UserLearningLessonController(ILogger<UserLearningLessonController> logger, ILearningService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy trạng thái học lesson theo enrollment, tự tạo user lesson nếu chưa tồn tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="lessonId">Mã lesson.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/lessons/{lessonId}</example>
    [HttpGet("{lessonId:guid}")]
    public async Task<IActionResult> GetOrCreateUserLesson(Guid enrollmentId, Guid lessonId)
    {
        try
        {
            var result = await _service.GetOrCreateUserLessonAsync(enrollmentId, lessonId);
            return Ok(SuccessResponse<UserLessonResponseDTO>.Create(result, "Lấy dữ liệu lesson học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy dữ liệu lesson học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Đánh dấu hoàn thành lesson lý thuyết.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="lessonId">Mã lesson lý thuyết.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/lessons/{lessonId}/complete</example>
    [HttpPost("{lessonId:guid}/complete")]
    public async Task<IActionResult> CompleteLesson(Guid enrollmentId, Guid lessonId)
    {
        try
        {
            var result = await _service.CompleteLessonAsync(enrollmentId, lessonId);
            return Ok(SuccessResponse<CompleteLessonResultDTO>.Create(result, "Hoàn thành lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Hoàn thành lesson thất bại.");
            throw;
        }
    }
}
