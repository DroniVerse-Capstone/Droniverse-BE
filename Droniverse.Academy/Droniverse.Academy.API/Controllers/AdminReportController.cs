using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/admin/reports")]
[ApiController]
[Authorize(Roles = Roles.AdminOrManagerRoles)]
/// <summary>
/// Quản lý report dành cho Admin/Manager.
/// </summary>
public class AdminReportController : ControllerBase
{
    private readonly ILogger<AdminReportController> _logger;
    private readonly IAdminReportService _service;

    public AdminReportController(ILogger<AdminReportController> logger, IAdminReportService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách report cho admin và manager.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="referenceId">Lọc theo reference.</param>
    /// <param name="userId">Lọc theo người gửi report.</param>
    /// <param name="reportType">Lọc theo loại report.</param>
    [HttpGet]
    public async Task<IActionResult> GetReports(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? referenceId = null,
        [FromQuery] Guid? userId = null,
        [FromQuery] ReportTypeFilter reportType = ReportTypeFilter.All)
    {
        try
        {
            var reports = await _service.GetReportsAsync(pageIndex, pageSize, referenceId, userId, MapToReportType(reportType));
            return Ok(SuccessResponse<PaginationResult<IEnumerable<ReportResponseDTO>>>.Create(reports, "Lấy danh sách report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách report thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết report.
    /// </summary>
    /// <param name="reportId">Mã report.</param>
    [HttpGet("{reportId:guid}")]
    public async Task<IActionResult> GetReportById(Guid reportId)
    {
        try
        {
            var report = await _service.GetReportByIdAsync(reportId);
            return Ok(SuccessResponse<ReportResponseDTO>.Create(report, "Lấy chi tiết report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết report thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Phản hồi report.
    /// </summary>
    /// <param name="reportId">Mã report.</param>
    /// <param name="request">Nội dung phản hồi.</param>
    [HttpPatch("{reportId:guid}/response")]
    [SwaggerRequestExample(typeof(RespondReportRequestDTO), typeof(RespondReportRequestExample))]
    public async Task<IActionResult> RespondReport(Guid reportId, [FromBody] RespondReportRequestDTO request)
    {
        try
        {
            var updated = await _service.RespondReportAsync(reportId, request);
            return Ok(SuccessResponse<ReportResponseDTO>.Create(updated, "Phản hồi report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Phản hồi report thất bại.");
            throw;
        }
    }

    private static ReportType? MapToReportType(ReportTypeFilter filter)
    {
        return filter switch
        {
            ReportTypeFilter.All => null,
            ReportTypeFilter.CourseVersion => ReportType.CourseVersion,
            ReportTypeFilter.Club => ReportType.Club,
            ReportTypeFilter.User => ReportType.User,
            _ => null
        };
    }

}
