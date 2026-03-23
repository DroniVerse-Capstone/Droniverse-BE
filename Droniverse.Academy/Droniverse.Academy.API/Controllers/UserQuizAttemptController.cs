using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/quiz-attempts")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserQuizAttemptController : ControllerBase
{
    private readonly ILogger<UserQuizAttemptController> _logger;
    private readonly IUserQuizAttemptService _service;

    public UserQuizAttemptController(ILogger<UserQuizAttemptController> logger, IUserQuizAttemptService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuizAttempt([FromBody] CreateUserQuizAttemptRequestDTO request)
    {
        try
        {
            var created = await _service.CreateUserQuizAttemptAsync(request);
            return StatusCode(201, SuccessResponse<object>.Create(created, "Tạo quiz attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo quiz attempt thất bại.");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuizAttempts(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserQuizAttemptResultFilter result = UserQuizAttemptResultFilter.All)
    {
        try
        {
            var data = await _service.GetMyQuizAttemptsAsync(pageIndex, pageSize, MapResultFilter(result));
            return Ok(SuccessResponse<object>.Create(data, "Lấy danh sách quiz attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách quiz attempt thất bại.");
            throw;
        }
    }

    [HttpGet("{attemptId:guid}")]
    public async Task<IActionResult> GetMyQuizAttemptById(Guid attemptId)
    {
        try
        {
            var data = await _service.GetMyQuizAttemptByIdAsync(attemptId);
            return Ok(SuccessResponse<object>.Create(data, "Lấy chi tiết quiz attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết quiz attempt thất bại.");
            throw;
        }
    }

    [HttpPut("{attemptId:guid}")]
    public async Task<IActionResult> UpdateMyQuizAttempt(Guid attemptId, [FromBody] UpdateUserQuizAttemptRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyQuizAttemptAsync(attemptId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật quiz attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật quiz attempt thất bại.");
            throw;
        }
    }

    [HttpDelete("{attemptId:guid}")]
    public async Task<IActionResult> DeleteMyQuizAttempt(Guid attemptId)
    {
        try
        {
            await _service.DeleteMyQuizAttemptAsync(attemptId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa quiz attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa quiz attempt thất bại.");
            throw;
        }
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
}
