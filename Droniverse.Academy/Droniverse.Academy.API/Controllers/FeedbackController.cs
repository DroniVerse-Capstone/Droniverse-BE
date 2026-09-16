using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/feedbacks")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private readonly ILogger<FeedbackController> _logger;
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(ILogger<FeedbackController> logger, IFeedbackService feedbackService)
    {
        _logger = logger;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Lấy danh sách phản hồi của toàn bộ khóa học (tất cả các phiên bản).
    /// </summary>
    [HttpGet("courses/{courseId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(SuccessResponse<IEnumerable<FeedbackClientViewDTO>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeedbacksByCourse(Guid courseId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByCourseAsync(courseId);
            return Ok(SuccessResponse<IEnumerable<FeedbackClientViewDTO>>.Create(feedbacks, "Lấy danh sách phản hồi khóa học thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách phản hồi khóa học thất bại.");
            throw;
        }
    }
}
