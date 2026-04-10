using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/lessons")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLessonController : ControllerBase
{
    private readonly ILogger<UserLessonController> _logger;
    private readonly IUserLessonService _service;

    public UserLessonController(ILogger<UserLessonController> logger, IUserLessonService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách user lesson của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái bài học.</param>
    [HttpGet]
    public async Task<IActionResult> GetMyUserLessons(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserLessonStatusFilter status = UserLessonStatusFilter.All)
    {
        try
        {
            var result = await _service.GetMyUserLessonsAsync(pageIndex, pageSize, MapUserLessonStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách user lesson thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết user lesson của người dùng hiện tại.
    /// </summary>
    /// <param name="userLessonId">Mã user lesson.</param>
    [HttpGet("{userLessonId:guid}")]
    public async Task<IActionResult> GetMyUserLessonById(Guid userLessonId)
    {
        try
        {
            var result = await _service.GetMyUserLessonByIdAsync(userLessonId);
            return Ok(SuccessResponse<object>.Create(result, "Lấy chi tiết user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết user lesson thất bại.");
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
}
