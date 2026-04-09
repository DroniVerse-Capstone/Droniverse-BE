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
    /// Lấy danh sách enrollment của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="status">Bộ lọc trạng thái enrollment.</param>
    [HttpGet]
    public async Task<IActionResult> GetMyEnrollments(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] EnrollmentStatusFilter status = EnrollmentStatusFilter.All)
    {
        try
        {
            var result = await _service.GetMyEnrollmentsAsync(pageIndex, pageSize, MapEnrollmentStatus(status));
            return Ok(SuccessResponse<PaginationResult<IEnumerable<EnrollmentResponseDTO>>>.Create(result, "Lấy danh sách enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết enrollment của người dùng hiện tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpGet("{enrollmentId:guid}")]
    public async Task<IActionResult> GetMyEnrollmentById(Guid enrollmentId)
    {
        try
        {
            var result = await _service.GetMyEnrollmentByIdAsync(enrollmentId);
            return Ok(SuccessResponse<EnrollmentResponseDTO>.Create(result, "Lấy chi tiết enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật enrollment của người dùng hiện tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="request">Thông tin enrollment cần cập nhật.</param>
    [HttpPatch("{enrollmentId:guid}")]
    [SwaggerRequestExample(typeof(UpdateEnrollmentRequestDTO), typeof(UpdateEnrollmentRequestExample))]
    public async Task<IActionResult> UpdateMyEnrollment(Guid enrollmentId, [FromBody] UpdateEnrollmentRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyEnrollmentAsync(enrollmentId, request);
            return Ok(SuccessResponse<EnrollmentResponseDTO>.Create(updated, "Cập nhật enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy learning path của enrollment hiện tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpGet("{enrollmentId:guid}/learning-path")]
    public async Task<IActionResult> GetMyLearningPath(Guid enrollmentId)
    {
        try
        {
            var result = await _service.GetMyLearningPathAsync(enrollmentId);
            return Ok(SuccessResponse<EnrollmentLearningPathResponseDTO>.Create(result, "Lấy learning path thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy learning path thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy bài học tiếp theo của enrollment hiện tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpGet("{enrollmentId:guid}/next")]
    public async Task<IActionResult> GetMyNextLesson(Guid enrollmentId)
    {
        try
        {
            var result = await _service.GetMyNextLessonAsync(enrollmentId);
            return Ok(SuccessResponse<EnrollmentNextLessonResponseDTO?>.Create(result, "Lấy bài học tiếp theo thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy bài học tiếp theo thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa enrollment của người dùng hiện tại.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpDelete("{enrollmentId:guid}")]
    public async Task<IActionResult> DeleteMyEnrollment(Guid enrollmentId)
    {
        try
        {
            await _service.DeleteMyEnrollmentAsync(enrollmentId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa enrollment thất bại.");
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
