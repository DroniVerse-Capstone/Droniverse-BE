using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/quiz-attempts")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
[ApiExplorerSettings(IgnoreApi = true)]
public class UserQuizAttemptController : ControllerBase
{
    private readonly ILogger<UserQuizAttemptController> _logger;
    private readonly IUserQuizAttemptService _service;

    public UserQuizAttemptController(ILogger<UserQuizAttemptController> logger, IUserQuizAttemptService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy chi tiết quiz attempt của người dùng hiện tại.
    /// </summary>
    /// <param name="attemptId">Mã quiz attempt.</param>
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
