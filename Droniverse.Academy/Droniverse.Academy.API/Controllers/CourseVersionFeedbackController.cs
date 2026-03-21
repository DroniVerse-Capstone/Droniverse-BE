using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/versions/{versionId:guid}/feedbacks")]
[ApiController]
public class CourseVersionFeedbackController : ControllerBase
{
    private readonly ILogger<CourseVersionFeedbackController> _logger;
    private readonly IFeedbackService _feedbackService;

    public CourseVersionFeedbackController(ILogger<CourseVersionFeedbackController> logger, IFeedbackService feedbackService)
    {
        _logger = logger;
        _feedbackService = feedbackService;
    }

    /// <summary>
    /// Tạo phản hồi cho phiên bản khóa học.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> CreateFeedback(Guid courseId, Guid versionId, [FromBody] FeedbackCreateDTO request)
    {
        try
        {
            var created = await _feedbackService.CreateFeedbackForCourseVersionAsync(courseId, versionId, request);
            return StatusCode(201,
                SuccessResponse<FeedbackClientViewDTO>.Create(
                    created,
                    "Gửi phản hồi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gửi phản hồi thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách phản hồi của phiên bản khóa học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{Roles.SystemManager},{Roles.ClubManager}")]
    public async Task<IActionResult> GetFeedbacks(Guid courseId, Guid versionId)
    {
        try
        {
            var feedbacks = await _feedbackService.GetFeedbacksByCourseVersionAsync(courseId, versionId);
            return Ok(SuccessResponse<IEnumerable<FeedbackClientViewDTO>>.Create(feedbacks, "Lấy danh sách phản hồi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách phản hồi thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết phản hồi.
    /// </summary>
    [HttpGet("{feedbackId:guid}")]
    [Authorize(Roles = $"{Roles.SystemManager},{Roles.ClubManager}")]
    public async Task<IActionResult> GetFeedbackDetail(Guid courseId, Guid versionId, Guid feedbackId)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackDetailAsync(courseId, versionId, feedbackId);
            return Ok(SuccessResponse<FeedbackClientViewDTO>.Create(feedback, "Lấy chi tiết phản hồi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết phản hồi thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật phản hồi.
    /// </summary>
    [HttpPut("{feedbackId:guid}")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> UpdateFeedback(Guid courseId, Guid versionId, Guid feedbackId, [FromBody] FeedbackUpdateDTO request)
    {
        try
        {
            var updated = await _feedbackService.UpdateFeedbackAsync(courseId, versionId, feedbackId, request);
            return Ok(SuccessResponse<FeedbackClientViewDTO>.Create(updated, "Cập nhật phản hồi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật phản hồi thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa phản hồi.
    /// </summary>
    [HttpDelete("{feedbackId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> DeleteFeedback(Guid courseId, Guid versionId, Guid feedbackId)
    {
        try
        {
            await _feedbackService.DeleteFeedbackAsync(courseId, versionId, feedbackId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa phản hồi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa phản hồi thất bại.");
            throw;
        }
    }

}
