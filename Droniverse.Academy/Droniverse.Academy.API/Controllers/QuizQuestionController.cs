using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("quizzes/{quizId:guid}/questions")]
[ApiController]
public class QuizQuestionController : ControllerBase
{
    private readonly ILogger<QuizQuestionController> _logger;
    private readonly IQuizQuestionService _quizQuestionService;

    public QuizQuestionController(ILogger<QuizQuestionController> logger, IQuizQuestionService quizQuestionService)
    {
        _logger = logger;
        _quizQuestionService = quizQuestionService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateQuizQuestion(Guid quizId, [FromBody] CreateQuizQuestionRequestDTO request)
    {
        try
        {
            var created = await _quizQuestionService.CreateQuizQuestionAsync(quizId, request);
            return StatusCode(201, SuccessResponse<QuizQuestionClientViewDTO>.Create(created, "Tạo câu hỏi quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo câu hỏi quiz thất bại.");
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetQuizQuestionsByQuizId(Guid quizId)
    {
        try
        {
            var questions = await _quizQuestionService.GetQuizQuestionsByQuizIdAsync(quizId);
            return Ok(SuccessResponse<IEnumerable<QuizQuestionClientViewDTO>>.Create(questions, "Lấy danh sách câu hỏi theo quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách câu hỏi theo quiz thất bại.");
            throw;
        }
    }

    [HttpGet("{questionId:guid}")]
    [Authorize(Roles = $"{Roles.AdminOrSystemManager},{Roles.ClubMember}")]
    public async Task<IActionResult> GetQuizQuestionById(Guid quizId, Guid questionId)
    {
        try
        {
            var question = await _quizQuestionService.GetQuizQuestionByIdAsync(quizId, questionId);
            return Ok(SuccessResponse<QuizQuestionClientViewDTO>.Create(question, "Lấy chi tiết câu hỏi quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết câu hỏi quiz thất bại.");
            throw;
        }
    }

    [HttpPut("{questionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> UpdateQuizQuestion(Guid quizId, Guid questionId, [FromBody] UpdateQuizQuestionRequestDTO request)
    {
        try
        {
            var updated = await _quizQuestionService.UpdateQuizQuestionAsync(quizId, questionId, request);
            return Ok(SuccessResponse<QuizQuestionClientViewDTO>.Create(updated, "Cập nhật câu hỏi quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật câu hỏi quiz thất bại.");
            throw;
        }
    }

    [HttpDelete("{questionId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteQuizQuestion(Guid quizId, Guid questionId)
    {
        try
        {
            await _quizQuestionService.DeleteQuizQuestionAsync(quizId, questionId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa câu hỏi quiz thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa câu hỏi quiz thất bại.");
            throw;
        }
    }
}
