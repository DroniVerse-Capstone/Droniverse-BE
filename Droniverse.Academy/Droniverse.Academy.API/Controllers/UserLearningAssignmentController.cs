using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments/{enrollmentId:guid}/assignments")]
[ApiController]
[Authorize(Roles = Roles.ClubMember)]
public class UserLearningAssignmentController : ControllerBase
{
    private readonly ILogger<UserLearningAssignmentController> _logger;
    private readonly IUserAssignmentService _service;

    public UserLearningAssignmentController(ILogger<UserLearningAssignmentController> logger, IUserAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("{assignmentId:guid}/attempts")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetMyAssignmentAttempts(
        Guid enrollmentId,
        Guid assignmentId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetMyAssignmentAttemptsAsync(enrollmentId, assignmentId, pageIndex, pageSize);
            return Ok(SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>.Create(
                result,
                "Lấy lịch sử nộp assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy lịch sử nộp assignment thất bại.");
            throw;
        }
    }
    /// <summary>
    /// Lấy đề kèm kết quả làm bài, nếu không có thì null
    /// </summary>
    /// <param name="enrollmentId"></param>
    /// <param name="assignmentId"></param>
    /// <returns></returns>
    [HttpGet("{assignmentId:guid}")]
    public async Task<IActionResult> GetAssignmentOverview (
        Guid enrollmentId,
        Guid assignmentId)
    {
        try
        {
            var result = await _service.GetAssignmentOverView(enrollmentId, assignmentId);
            return Ok(SuccessResponse<AssignmentOverview>.Create(
                result,
                "Lấy assignment overview thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy assignment overview thất bại.");
            throw;
        }
    }

    [HttpPost("{assignmentId:guid}/submit")]
    [SwaggerRequestExample(typeof(SubmitUserAssignmentRequestDTO), typeof(SubmitUserAssignmentRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentSubmitSuccessResponseExample))]
    public async Task<IActionResult> SubmitAssignment(
        Guid enrollmentId,
        Guid assignmentId,
        [FromBody] SubmitUserAssignmentRequestDTO request)
    {
        try
        {
            var result = await _service.SubmitAssignmentAsync(enrollmentId, assignmentId, request);
            return Ok(SuccessResponse<UserAssignmentSubmitResponseDTO>.Create(result, "Nộp assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nộp assignment thất bại.");
            throw;
        }
    }
}
