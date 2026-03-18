using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/quizzes")]
[ApiController]
public class QuizController : ControllerBase
{
    private readonly ILogger<QuizController> _logger;
    private readonly IQuizService _quizService;

    public QuizController(ILogger<QuizController> logger, IQuizService quizService)
    {
        _logger = logger;
        _quizService = quizService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequestDTO request)
    {
        try
        {
            var created = await _quizService.CreateQuizAsync(request);
            return StatusCode(201, SuccessResponse<QuizClientViewDTO>.Create(created, "Tạo quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo quiz thất bại.");
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetQuizzes()
    {
        try
        {
            var quizzes = await _quizService.GetQuizzesAsync();
            return Ok(SuccessResponse<IEnumerable<QuizClientViewDTO>>.Create(quizzes, "Lấy danh sách quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách quiz thất bại.");
            throw;
        }
    }

    [HttpGet("{quizId:guid}")]
    [Authorize(Roles = $"{Roles.AdminOrSystemManager},{Roles.ClubMember}")]
    public async Task<IActionResult> GetQuizById(Guid quizId)
    {
        try
        {
            var quiz = await _quizService.GetQuizByIdAsync(quizId);
            return Ok(SuccessResponse<QuizClientViewDTO>.Create(quiz, "Lấy chi tiết quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết quiz thất bại.");
            throw;
        }
    }

    [HttpPut("{quizId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateQuiz(Guid quizId, [FromBody] UpdateQuizRequestDTO request)
    {
        try
        {
            var updated = await _quizService.UpdateQuizAsync(quizId, request);
            return Ok(SuccessResponse<QuizClientViewDTO>.Create(updated, "Cập nhật quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật quiz thất bại.");
            throw;
        }
    }

    [HttpDelete("{quizId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteQuiz(Guid quizId)
    {
        try
        {
            await _quizService.DeleteQuizAsync(quizId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa quiz thất bại.");
            throw;
        }
    }
}
