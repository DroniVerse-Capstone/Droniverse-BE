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
