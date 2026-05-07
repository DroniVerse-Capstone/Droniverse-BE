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

    /// <summary>
    /// Chuyển tất cả enrollment của user sang LIMITED_ACCESS và khóa các user lesson.
    /// </summary>
    /// <param name="userId">ID của user.</param>
    [HttpPatch("users/{userId:guid}/limit-access")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> LimitUserAccess(Guid userId)
    {
        try
        {
            var result = await _service.LimitUserAccessAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật enrollment và user lesson thất bại.");
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
