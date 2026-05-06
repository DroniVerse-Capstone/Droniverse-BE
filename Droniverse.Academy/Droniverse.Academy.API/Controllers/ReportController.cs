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

[Route("academy/reports")]
[ApiController]
[Authorize(Roles = Roles.ClubRoles)]
public class ReportController : ControllerBase
{
    private readonly ILogger<ReportController> _logger;
    private readonly IReportService _service;

    public ReportController(ILogger<ReportController> logger, IReportService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Tạo báo cáo mới.
    /// </summary>
    /// <param name="request">Thông tin báo cáo cần tạo.</param>
    [HttpPost]
    [SwaggerRequestExample(typeof(CreateReportRequestDTO), typeof(CreateReportRequestExample))]
    public async Task<IActionResult> CreateReport([FromBody] CreateReportRequestDTO request)
    {
        try
        {
            var created = await _service.CreateReportAsync(request);
            return StatusCode(201, SuccessResponse<ReportResponseDTO>.Create(created, "Tạo report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo report thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách báo cáo của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="reportType">Lọc theo loại report.</param>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReports(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] ReportTypeFilter reportType = ReportTypeFilter.All)
    {
        try
        {
            var reports = await _service.GetMyReportsAsync(pageIndex, pageSize, MapToReportType(reportType));
            return Ok(SuccessResponse<PaginationResult<IEnumerable<ReportResponseDTO>>>.Create(reports, "Lấy danh sách report của tôi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách report của tôi thất bại.");
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

    /// <summary>
    /// Lấy chi tiết một báo cáo.
    /// </summary>
    /// <param name="reportId">Mã báo cáo.</param>
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
}
