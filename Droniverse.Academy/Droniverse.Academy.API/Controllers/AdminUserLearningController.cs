using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/admin/users/{userId:guid}/learning")]
[ApiController]
[Authorize(Roles = Roles.AdminOrManagerRoles)]
/// <summary>
/// Theo dõi dữ liệu học tập của người dùng dành cho Admin/Manager.
/// </summary>
public class AdminUserLearningController : ControllerBase
{
    private readonly ILogger<AdminUserLearningController> _logger;
    private readonly IAdminUserLearningService _service;

    public AdminUserLearningController(ILogger<AdminUserLearningController> logger, IAdminUserLearningService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách bài lab của người dùng.
    /// </summary>
    [HttpGet("labs")]
    public async Task<IActionResult> GetUserLabs(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserLabCompletionFilter completion = UserLabCompletionFilter.All)
    {
        try
        {
            var result = await _service.GetUserLabsAsync(userId, pageIndex, pageSize, MapCompletionFilter(completion));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách bài lab của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách bài lab của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách quiz attempt của người dùng.
    /// </summary>
    [HttpGet("quiz-attempts")]
    public async Task<IActionResult> GetUserQuizAttempts(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserQuizAttemptResultFilter result = UserQuizAttemptResultFilter.All)
    {
        try
        {
            var data = await _service.GetUserQuizAttemptsAsync(userId, pageIndex, pageSize, MapResultFilter(result));
            return Ok(SuccessResponse<object>.Create(data, "Lấy danh sách quiz attempt của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách quiz attempt của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách quiz question attempt của người dùng.
    /// </summary>
    [HttpGet("quiz-question-attempts")]
    public async Task<IActionResult> GetUserQuizQuestionAttempts(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserQuizQuestionAttemptCorrectFilter correctness = UserQuizQuestionAttemptCorrectFilter.All)
    {
        try
        {
            var data = await _service.GetUserQuizQuestionAttemptsAsync(userId, pageIndex, pageSize, MapCorrectnessFilter(correctness));
            return Ok(SuccessResponse<object>.Create(data, "Lấy danh sách quiz question attempt của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách quiz question attempt của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách module học của người dùng.
    /// </summary>
    [HttpGet("modules")]
    public async Task<IActionResult> GetUserModules(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserModuleCompletionFilter completion = UserModuleCompletionFilter.All)
    {
        try
        {
            var result = await _service.GetUserModulesAsync(userId, pageIndex, pageSize, MapCompletionFilter(completion));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách module của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách module của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách lesson học của người dùng.
    /// </summary>
    [HttpGet("lessons")]
    public async Task<IActionResult> GetUserLessons(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserLessonStatusFilter status = UserLessonStatusFilter.All)
    {
        try
        {
            var result = await _service.GetUserLessonsAsync(userId, pageIndex, pageSize, MapUserLessonStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách lesson của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách lesson của người dùng thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách enrollment của người dùng.
    /// </summary>
    [HttpGet("enrollments")]
    public async Task<IActionResult> GetUserEnrollments(
        Guid userId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] EnrollmentStatusFilter status = EnrollmentStatusFilter.All)
    {
        try
        {
            var result = await _service.GetUserEnrollmentsAsync(userId, pageIndex, pageSize, MapEnrollmentStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách enrollment của người dùng thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách enrollment của người dùng thất bại.");
            throw;
        }
    }

    private static bool? MapCompletionFilter(UserLabCompletionFilter completion)
    {
        return completion switch
        {
            UserLabCompletionFilter.All => null,
            UserLabCompletionFilter.Incompleted => false,
            UserLabCompletionFilter.Completed => true,
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

    private static bool? MapResultFilter(UserQuizAttemptResultFilter result)
    {
        return result switch
        {
            UserQuizAttemptResultFilter.All => null,
            UserQuizAttemptResultFilter.Failed => false,
            UserQuizAttemptResultFilter.Passed => true,
            _ => null
        };
    }

    private static bool? MapCorrectnessFilter(UserQuizQuestionAttemptCorrectFilter correctness)
    {
        return correctness switch
        {
            UserQuizQuestionAttemptCorrectFilter.All => null,
            UserQuizQuestionAttemptCorrectFilter.Incorrect => false,
            UserQuizQuestionAttemptCorrectFilter.Correct => true,
            _ => null
        };
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

    private static EnrollStatus? MapEnrollmentStatus(EnrollmentStatusFilter status)
    {
        return status switch
        {
            EnrollmentStatusFilter.All => null,
            EnrollmentStatusFilter.Dropped => EnrollStatus.DROPPED,
            EnrollmentStatusFilter.Active => EnrollStatus.ACTIVE,
            EnrollmentStatusFilter.Completed => EnrollStatus.COMPLETED,
            EnrollmentStatusFilter.LimitedAccess => EnrollStatus.LIMITED_ACCESS,
            _ => null
        };
    }
}
