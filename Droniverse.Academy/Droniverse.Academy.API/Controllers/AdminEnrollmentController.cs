using Droniverse.Academy.API.Enums;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/admin/enrollments")]
[ApiController]
[Authorize(Roles = Roles.AdminOrManagerRoles)]
/// <summary>
/// Quản lý enrollment dành cho Admin/Manager.
/// </summary>
public class AdminEnrollmentController : ControllerBase
{
    private readonly ILogger<AdminEnrollmentController> _logger;
    private readonly IAdminEnrollmentService _service;

    public AdminEnrollmentController(ILogger<AdminEnrollmentController> logger, IAdminEnrollmentService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách enrollment cho admin và manager.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="userId">Lọc theo người dùng.</param>
    /// <param name="courseVersionId">Lọc theo phiên bản khóa học.</param>
    /// <param name="status">Lọc theo trạng thái enrollment.</param>
    [HttpGet]
    public async Task<IActionResult> GetEnrollments(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? userId = null,
        [FromQuery] Guid? courseVersionId = null,
        [FromQuery] EnrollmentStatusFilter status = EnrollmentStatusFilter.All)
    {
        try
        {
            var result = await _service.GetEnrollmentsAsync(pageIndex, pageSize, userId, courseVersionId, MapEnrollmentStatus(status));
            return Ok(SuccessResponse<object>.Create(result, "Lấy danh sách enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết enrollment.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpGet("{enrollmentId:guid}")]
    public async Task<IActionResult> GetEnrollmentById(Guid enrollmentId)
    {
        try
        {
            var result = await _service.GetEnrollmentByIdAsync(enrollmentId);
            return Ok(SuccessResponse<EnrollmentResponseDTO>.Create(result, "Lấy chi tiết enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật enrollment.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    /// <param name="request">Dữ liệu cập nhật enrollment.</param>
    [HttpPatch("{enrollmentId:guid}")]
    [SwaggerRequestExample(typeof(AdminUpdateEnrollmentRequestDTO), typeof(AdminUpdateEnrollmentRequestExample))]
    public async Task<IActionResult> UpdateEnrollment(Guid enrollmentId, [FromBody] AdminUpdateEnrollmentRequestDTO request)
    {
        try
        {
            var result = await _service.UpdateEnrollmentAsync(enrollmentId, request);
            return Ok(SuccessResponse<EnrollmentResponseDTO>.Create(result, "Cập nhật enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật enrollment thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa enrollment.
    /// </summary>
    /// <param name="enrollmentId">Mã enrollment.</param>
    [HttpDelete("{enrollmentId:guid}")]
    public async Task<IActionResult> DeleteEnrollment(Guid enrollmentId)
    {
        try
        {
            await _service.DeleteEnrollmentAsync(enrollmentId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa enrollment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa enrollment thất bại.");
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
