using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/quizzes")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserLearningQuizController : ControllerBase
{
    private readonly ILogger<UserLearningQuizController> _logger;
    private readonly IQuizLearningService _service;

    public UserLearningQuizController(ILogger<UserLearningQuizController> logger, IQuizLearningService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy dữ liệu làm quiz của người học theo quiz.
    /// Trả về cấu trúc gồm thông tin quiz và attempt mới nhất của người học (nếu có).
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment của người học.</param>
    /// <param name="quizId">Mã quiz.</param>
    [HttpGet("{quizId:guid}")]
    public async Task<IActionResult> GetQuizAttemptOrQuiz(Guid enrollmentId, Guid quizId)
    {
        try
        {
            var result = await _service.GetQuizAttemptOrQuizAsync(enrollmentId, quizId);
            return Ok(SuccessResponse<QuizLearningStateDTO>.Create(result, "Lấy dữ liệu quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy dữ liệu quiz thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách câu hỏi quiz cho người học, đã đảo thứ tự câu hỏi và đáp án để làm bài.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment của người học.</param>
    /// <param name="quizId">Mã quiz.</param>
    [HttpGet("{quizId:guid}/questions")]
    public async Task<IActionResult> GetQuizQuestionsForLearning(Guid enrollmentId, Guid quizId)
    {
        try
        {
            var result = await _service.GetQuizQuestionsForLearningAsync(enrollmentId, quizId);
            return Ok(SuccessResponse<IEnumerable<QuizQuestionLearningDTO>>.Create(result, "Lấy danh sách câu hỏi quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách câu hỏi quiz thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Nộp bài quiz của người học.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment của người học.</param>
    /// <param name="quizId">Mã quiz.</param>
    /// <param name="request">Danh sách câu trả lời người học gửi lên.</param>
    [HttpPost("{quizId:guid}/submit")]
    [SwaggerRequestExample(typeof(SubmitQuizRequestDTO), typeof(SubmitQuizRequestExample))]
    public async Task<IActionResult> SubmitQuiz(Guid enrollmentId, Guid quizId, [FromBody] SubmitQuizRequestDTO request)
    {
        try
        {
            var result = await _service.SubmitQuizAsync(enrollmentId, quizId, request);
            return Ok(SuccessResponse<SubmitQuizResultDTO>.Create(result, "Nộp quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nộp quiz thất bại.");
            throw;
        }
    }
}
