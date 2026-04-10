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

    /// <summary>
    /// Lấy danh sách user lesson của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái lesson.</param>
    /// <example>/academy/user/lessons?pageIndex=1&amp;pageSize=10&amp;status=All</example>
    [HttpGet("/academy/user/lessons")]
    public async Task<IActionResult> GetMyUserLessons(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserLessonStatusFilter status = UserLessonStatusFilter.All)
    {
        try
        {
            var result = await _userLessonService.GetMyUserLessonsAsync(pageIndex, pageSize, MapUserLessonStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách user lesson thất bại.");
            throw;
        }
    }


    /// <summary>
    /// Lấy danh sách user module của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="completion">Bộ lọc trạng thái hoàn thành module.</param>
    /// <example>/academy/user/modules?pageIndex=1&amp;pageSize=10&amp;completion=All</example>
    [HttpGet("/academy/user/modules")]
    public async Task<IActionResult> GetMyUserModules(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserModuleCompletionFilter completion = UserModuleCompletionFilter.All)
    {
        try
        {
            var result = await _userModuleService.GetMyUserModulesAsync(pageIndex, pageSize, MapCompletionFilter(completion));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách user module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách user module thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy trạng thái học module theo enrollment, tự tạo user module nếu chưa tồn tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="moduleId">Mã module.</param>
    /// <example>/academy/user/enrollments/{enrollmentId}/modules/{moduleId}</example>
    [HttpGet("/academy/user/enrollments/{enrollmentId:guid}/modules/{moduleId:guid}")]
    public async Task<IActionResult> GetOrCreateUserModule(Guid enrollmentId, Guid moduleId)
    {
        try
        {
            var result = await _learningService.GetOrCreateUserModuleAsync(enrollmentId, moduleId);
            return Ok(SuccessResponse<UserModuleResponseDTO>.Create(result, "Lấy dữ liệu module học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy dữ liệu module học thất bại.");
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
