using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/learning-path")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLearningController : ControllerBase
{
    private readonly ILogger<UserLearningController> _logger;
    private readonly ILearningService _service;

    public UserLearningController(ILogger<UserLearningController> logger, ILearningService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetLearningPath(Guid enrollmentId)
    {
        try
        {
            var result = await _service.GetMyLearningPathAsync(enrollmentId);
            return Ok(SuccessResponse<LearningPathDTO>.Create(result, "Lấy learning path thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy learning path thất bại.");
            throw;
        }
    }
}
