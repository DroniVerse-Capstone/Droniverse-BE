using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

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

    [HttpPost]
    [SwaggerRequestExample(typeof(CreateUserLessonRequestDTO), typeof(CreateUserLessonRequestExample))]
    public async Task<IActionResult> CreateUserLesson([FromBody] CreateUserLessonRequestDTO request)
    {
        try
        {
            var created = await _service.CreateUserLessonAsync(request);
            return StatusCode(201, SuccessResponse<object>.Create(created, "Tạo user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo user lesson thất bại.");
            throw;
        }
    }

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

    [HttpPut("{userLessonId:guid}")]
    [SwaggerRequestExample(typeof(UpdateUserLessonRequestDTO), typeof(UpdateUserLessonRequestExample))]
    public async Task<IActionResult> UpdateMyUserLesson(Guid userLessonId, [FromBody] UpdateUserLessonRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyUserLessonAsync(userLessonId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật user lesson thất bại.");
            throw;
        }
    }

    [HttpDelete("{userLessonId:guid}")]
    public async Task<IActionResult> DeleteMyUserLesson(Guid userLessonId)
    {
        try
        {
            await _service.DeleteMyUserLessonAsync(userLessonId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa user lesson thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa user lesson thất bại.");
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
