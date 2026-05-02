using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/club/enrollments")]
[ApiController]
[Authorize(Roles = Roles.ClubManager)]
public class ClubEnrollmentController : ControllerBase
{
    private readonly ILogger<ClubEnrollmentController> _logger;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILearningService _learningService;

    public ClubEnrollmentController(
        ILogger<ClubEnrollmentController> logger,
        IEnrollmentService enrollmentService,
        ILearningService learningService)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _learningService = learningService;
    }

    /// <summary>
    /// Lấy danh sách enrollment trong club, sắp xếp theo level khóa học, có lọc theo khóa học hoặc user
    /// </summary>
    /// <param name="clubId">ID của club</param>
    /// <param name="courseId">ID của khóa học (tùy chọn)</param>
    /// <param name="userId">ID của user (tùy chọn)</param>
    /// <param name="pageIndex">Trang hiện tại (mặc định: 1)</param>
    /// <param name="pageSize">Số lượng phần tử mỗi trang (mặc định: 10)</param>
    /// <returns>Danh sách enrollment với thông tin tiến độ (phần trăm)</returns>
    [HttpGet("{clubId:guid}")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetEnrollmentsByClub(
        [FromRoute] Guid clubId,
        [FromQuery] Guid? courseId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _enrollmentService.GetEnrollmentsByClubAsync(
                clubId,
                pageIndex,
                pageSize,
                courseId,
                userId);

            return Ok(SuccessResponse<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>>.Create(
                result,
                "Lấy danh sách enrollment trong club thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách enrollment trong club thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy learning path của một user, bao gồm tất cả module, lesson, và trạng thái hoàn thành
    /// </summary>
    /// <param name="enrollmentId">ID của enrollment</param>
    /// <returns>Learning path chi tiết với tiến độ</returns>
    [HttpGet("learning-path/{enrollmentId:guid}")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(UserAssignmentAttemptsSuccessResponseExample))]
    public async Task<IActionResult> GetUserLearningPath([FromRoute] Guid enrollmentId)
    {
        try
        {
            var result = await _learningService.GetLearningPathAsync(enrollmentId);

            return Ok(SuccessResponse<object>.Create(
                result,
                "Lấy learning path thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy learning path thất bại.");
            throw;
        }
    }
}
