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
    /// Lấy danh sách lesson VR đang học dở (INCOMPLETED) của người dùng hiện tại.
    /// </summary>
    /// <example>/academy/user/lessons/vrs/incompleted</example>
    [HttpGet("~/academy/user/lessons/vrs/incompleted")]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<IncompleteVRLessonResponseDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListVRs()
    {
        try
        {
            var result = await _service.GetListVRsAsync();
            return Ok(SuccessResponse<IEnumerable<IncompleteVRLessonResponseDTO>>.Create(result, "Lấy danh sách lesson VR đang học dở thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách lesson VR đang học dở thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Tạo dữ liệu học lesson theo enrollment.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="lessonId">Mã lesson.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/lessons/{lessonId}</example>
    /// <example>/academy/user/enrollments/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222</example>
    [HttpPost("{lessonId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse<UserLessonResponseDTO>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUserLesson(Guid enrollmentId, Guid lessonId)
    {
        try
        {
            var result = await _service.CreateUserLessonAsync(enrollmentId, lessonId);
            return StatusCode(201, SuccessResponse<UserLessonResponseDTO>.Create(result, "Tạo dữ liệu lesson học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo dữ liệu lesson học thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Kiểm tra user lesson đã tồn tại theo lesson.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="lessonId">Mã lesson.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/lessons/{lessonId}/exists</example>
    /// <example>/academy/user/enrollments/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/exists</example>
    [HttpGet("{lessonId:guid}/exists")]
    [ProducesResponseType(typeof(SuccessResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckUserLessonExists(Guid enrollmentId, Guid lessonId)
    {
        try
        {
            var result = await _service.CheckUserLessonExistsAsync(enrollmentId, lessonId);
            return Ok(SuccessResponse<bool>.Create(result, "Kiểm tra user lesson tồn tại thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kiểm tra user lesson tồn tại thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Đánh dấu hoàn thành lesson trực tiếp.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="lessonId">Mã lesson (THEORY, WEB, VR).</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/lessons/{lessonId}/complete</example>
    /// <example>/academy/user/enrollments/11111111-1111-1111-1111-111111111111/lessons/22222222-2222-2222-2222-222222222222/complete</example>
    [HttpPost("{lessonId:guid}/complete")]
    [ProducesResponseType(typeof(SuccessResponse<CompleteLessonResultDTO>), StatusCodes.Status200OK)]
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
