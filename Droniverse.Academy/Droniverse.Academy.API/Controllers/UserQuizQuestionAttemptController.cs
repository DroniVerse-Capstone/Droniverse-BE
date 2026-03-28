using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/quiz-question-attempts")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserQuizQuestionAttemptController : ControllerBase
{
    private readonly ILogger<UserQuizQuestionAttemptController> _logger;
    private readonly IUserQuizQuestionAttemptService _service;

    public UserQuizQuestionAttemptController(ILogger<UserQuizQuestionAttemptController> logger, IUserQuizQuestionAttemptService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    [SwaggerRequestExample(typeof(CreateUserQuizQuestionAttemptRequestDTO), typeof(CreateUserQuizQuestionAttemptRequestExample))]
    public async Task<IActionResult> CreateQuizQuestionAttempt([FromBody] CreateUserQuizQuestionAttemptRequestDTO request)
    {
        try
        {
            var created = await _service.CreateUserQuizQuestionAttemptAsync(request);
            return StatusCode(201, SuccessResponse<object>.Create(created, "Tạo quiz question attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo quiz question attempt thất bại.");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyQuizQuestionAttempts(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserQuizQuestionAttemptCorrectFilter correctness = UserQuizQuestionAttemptCorrectFilter.All)
    {
        try
        {
            var data = await _service.GetMyQuizQuestionAttemptsAsync(pageIndex, pageSize, MapCorrectnessFilter(correctness));
            return Ok(SuccessResponse<object>.Create(data, "Lấy danh sách quiz question attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách quiz question attempt thất bại.");
            throw;
        }
    }

    [HttpGet("{attemptAnswerId:guid}")]
    public async Task<IActionResult> GetMyQuizQuestionAttemptById(Guid attemptAnswerId)
    {
        try
        {
            var data = await _service.GetMyQuizQuestionAttemptByIdAsync(attemptAnswerId);
            return Ok(SuccessResponse<object>.Create(data, "Lấy chi tiết quiz question attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết quiz question attempt thất bại.");
            throw;
        }
    }

    [HttpPut("{attemptAnswerId:guid}")]
    [SwaggerRequestExample(typeof(UpdateUserQuizQuestionAttemptRequestDTO), typeof(UpdateUserQuizQuestionAttemptRequestExample))]
    public async Task<IActionResult> UpdateMyQuizQuestionAttempt(Guid attemptAnswerId, [FromBody] UpdateUserQuizQuestionAttemptRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyQuizQuestionAttemptAsync(attemptAnswerId, request);
            return Ok(SuccessResponse<object>.Create(updated, "Cập nhật quiz question attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật quiz question attempt thất bại.");
            throw;
        }
    }

    [HttpDelete("{attemptAnswerId:guid}")]
    public async Task<IActionResult> DeleteMyQuizQuestionAttempt(Guid attemptAnswerId)
    {
        try
        {
            await _service.DeleteMyQuizQuestionAttemptAsync(attemptAnswerId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa quiz question attempt thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa quiz question attempt thất bại.");
            throw;
        }
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
}
