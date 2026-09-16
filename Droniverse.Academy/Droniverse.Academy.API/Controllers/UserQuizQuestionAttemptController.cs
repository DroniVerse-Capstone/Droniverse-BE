using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/quiz-question-attempts")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
[ApiExplorerSettings(IgnoreApi = true)]
public class UserQuizQuestionAttemptController : ControllerBase
{
    private readonly ILogger<UserQuizQuestionAttemptController> _logger;
    private readonly IUserQuizQuestionAttemptService _service;

    public UserQuizQuestionAttemptController(ILogger<UserQuizQuestionAttemptController> logger, IUserQuizQuestionAttemptService service)
    {
        _logger = logger;
        _service = service;
    }


    /// <summary>
    /// Lấy chi tiết quiz question attempt của người dùng hiện tại.
    /// </summary>
    /// <param name="attemptAnswerId">Mã quiz question attempt.</param>
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
