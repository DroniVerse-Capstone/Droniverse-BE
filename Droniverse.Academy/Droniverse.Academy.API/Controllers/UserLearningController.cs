using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Domain.Enums;
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
    private readonly ILearningService _learningService;
    private readonly IUserLessonService _userLessonService;
    private readonly IUserModuleService _userModuleService;

    public UserLearningController(
        ILogger<UserLearningController> logger,
        ILearningService learningService,
        IUserLessonService userLessonService,
        IUserModuleService userModuleService)
    {
        _logger = logger;
        _learningService = learningService;
        _userLessonService = userLessonService;
        _userModuleService = userModuleService;
    }

    /// <summary>
    /// Lấy learning path hiện tại của người học theo enrollment.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/learning-path</example>
    [HttpGet]
    public async Task<IActionResult> GetLearningPath(Guid enrollmentId)
    {
        try
        {
            var result = await _learningService.GetMyLearningPathAsync(enrollmentId);
            return Ok(SuccessResponse<LearningPathDTO>.Create(result, "Lấy learning path thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy learning path thất bại.");
            throw;
        }
    }


    private static UserLessonStatus? MapUserLessonStatus(UserLessonStatusFilter status)
    {
        return status switch
        {
            UserLessonStatusFilter.All => null,
            UserLessonStatusFilter.Incompleted => UserLessonStatus.INCOMPLETED,
            UserLessonStatusFilter.Completed => UserLessonStatus.COMPLETED,
            UserLessonStatusFilter.Locked => UserLessonStatus.LOCKED,
            _ => null
        };
    }

    private static bool? MapCompletionFilter(UserModuleCompletionFilter completion)
    {
        return completion switch
        {
            UserModuleCompletionFilter.All => null,
            UserModuleCompletionFilter.Incompleted => false,
            UserModuleCompletionFilter.Completed => true,
            _ => null
        };
    }
}
