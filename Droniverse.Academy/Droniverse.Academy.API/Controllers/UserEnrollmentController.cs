using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Extension;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/user/enrollments")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
public class UserEnrollmentController : ControllerBase
{
    private readonly ILogger<UserEnrollmentController> _logger;
    private readonly IEnrollmentService _service;

    public UserEnrollmentController(ILogger<UserEnrollmentController> logger, IEnrollmentService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Tạo enrollment cho người dùng hiện tại.
    /// </summary>
    /// <param name="request">Thông tin enrollment cần tạo.</param>
    [HttpPost]
    [SwaggerRequestExample(typeof(CreateEnrollmentRequestDTO), typeof(CreateEnrollmentRequestExample))]
    public async Task<IActionResult> CreateEnrollment([FromBody] CreateEnrollmentRequestDTO request)
    {
        try
        {
            var created = await _service.CreateEnrollmentAsync(request);
            return StatusCode(201, SuccessResponse<EnrollmentResponseDTO>.Create(created, "Tạo enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết enrollment của người dùng hiện tại theo club và course version.
    /// </summary>
    /// <param name="clubId">Mã câu lạc bộ.</param>
    /// <param name="courseVersionId">Mã phiên bản khóa học.</param>
    [HttpGet("me/clubs/{clubId:guid}/course-versions/{courseVersionId:guid}")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<IActionResult> GetMyEnrollmentByClubAndCourseVersion(Guid clubId, Guid courseVersionId)
    {
        try
        {
            var result = await _service.GetMyEnrollmentByClubAndCourseVersionAsync(clubId, courseVersionId);
            return Ok(SuccessResponse<EnrollmentResponseDTO>.Create(result, "Lấy chi tiết enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết enrollment theo club và course version thất bại.");
            throw;
        }
    }


    /// <summary>
    /// Lấy ra danh sách khóa học cùng với tiến trình
    /// </summary>
    /// <remarks>
    /// - dùng cho ROLE : <b>CLUB_MEMBER</b>
    /// - Để <b>EnrollmentStatus</b> mặc định => lấy status <b>ACTIVE</b>
    /// </remarks>
    /// <param name="clubId">ID của câu lạc bộ</param>
    /// <param name="request">Search request của api</param>
    /// <returns></returns>
    [HttpGet("me/clubs/{clubId}/courses")]
    [Authorize(Roles = Roles.ClubMember)]
    public async Task<ApiResponse> GetCoursesOfUser(Guid clubId, [FromQuery] UserEnrollmentSearchRequest request)
    {
        try
        {
            var result = await _service.GetCoursesOfUser(clubId, request);
            return SuccessResponse<PaginationResult<IEnumerable<CoursesEnrollmentResponse>>>.Create(
                result,
                "Lấy danh sách khóa học của người dùng thành công.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách khóa học của người dùng thất bại.");
            throw;
        }
    }

    private static EnrollStatus? MapEnrollmentStatus(EnrollmentStatusFilter status)
    {
        return status switch
        {
            EnrollmentStatusFilter.All => null,
            EnrollmentStatusFilter.Dropped => EnrollStatus.DROPPED,
            EnrollmentStatusFilter.Active => EnrollStatus.ACTIVE,
            EnrollmentStatusFilter.Completed => EnrollStatus.COMPLETED,
            EnrollmentStatusFilter.LimitedAccess => EnrollStatus.LIMITED_ACCESS,
            _ => null
        };
    }

}
