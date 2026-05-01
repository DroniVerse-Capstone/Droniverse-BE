using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/manager/assignments/submissions")]
[ApiController]
[Authorize(Roles = Roles.ClubManager)]
public class UserAssignmentController : ControllerBase
{
    private readonly ILogger<UserAssignmentController> _logger;
    private readonly IUserAssignmentService _service;

    public UserAssignmentController(ILogger<UserAssignmentController> logger, IUserAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] Guid? assignmentId = null,
        [FromQuery] Guid? enrollmentId = null,
        [FromQuery] UserAssignmentStatus? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetSubmissionsForReviewAsync(
                assignmentId,
                enrollmentId,
                status,
                pageIndex,
                pageSize);

            return Ok(SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>.Create(
                result,
                "Lấy danh sách submission assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách submission assignment thất bại.");
            throw;
        }
    }
    /// <summary>
    /// Cho admin dùng, có thể lọc theo courseId hoặc clubId hoặc cả 2
    /// </summary>
    /// <param name="courseId"></param>
    /// <param name="clubId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("attempts")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetAssignmentAttempts(
        [FromQuery] Guid? courseId = null,
        [FromQuery] Guid? clubId = null,
        [FromQuery] Domain.Enums.UserAssignmentStatus? status = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetAssignmentAttemptsByCourseAndClubAsync(
                courseId,
                clubId,
                status,
                pageIndex,
                pageSize);

            return Ok(SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>.Create(
                result,
                "Lấy danh sách attempt assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách attempt assignment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cho club manager dùng
    /// </summary>
    /// <param name="courseId"></param>
    /// <param name="clubId"></param>
    /// <param name="pageIndex"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    [HttpGet("attempts/club/{clubId:guid}")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetAssignmentAttempts(
        [FromRoute] Guid clubId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? courseId = null,
        [FromQuery] Domain.Enums.UserAssignmentStatus? status = null)
    {
        try
        {
            var result = await _service.GetAssignmentAttemptsByCourseAndClubAsync(
                courseId,
                clubId,
                status,
                pageIndex,
                pageSize);

            return Ok(SuccessResponse<PaginationResult<IEnumerable<UserAssignmentAttemptResponseDTO>>>.Create(
                result,
                "Lấy danh sách attempt assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách attempt assignment thất bại.");
            throw;
        }
    }

    [HttpPost("{userAssignmentId:guid}/review")]
    [SwaggerRequestExample(typeof(ReviewUserAssignmentRequestDTO), typeof(ReviewUserAssignmentRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentReviewSuccessResponseExample))]
    public async Task<IActionResult> ReviewAssignment(Guid userAssignmentId, [FromBody] ReviewUserAssignmentRequestDTO request)
    {
        try
        {
            var result = await _service.ReviewAssignmentAsync(userAssignmentId, request);
            return Ok(SuccessResponse<UserAssignmentReviewResponseDTO>.Create(result, "Chấm assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Chấm assignment thất bại.");
            throw;
        }
    }
}
