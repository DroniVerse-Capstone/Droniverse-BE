using Droniverse.Academy.API.Examples;
using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/reports")]
[ApiController]
[Authorize(Roles = Roles.AllRoles)]
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
    /// Lấy danh sách báo cáo và lọc theo reference hoặc người dùng.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    /// <param name="referenceId">Mã reference cần lọc.</param>
    /// <param name="userId">Mã người dùng cần lọc.</param>
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrManagerRoles)]
    public async Task<IActionResult> GetReports(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? referenceId = null,
        [FromQuery] Guid? userId = null)
    {
        try
        {
            var reports = await _service.GetReportsAsync(pageIndex, pageSize, referenceId, userId);
            return Ok(SuccessResponse<object>.Create(reports, "Lấy danh sách report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách report thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách báo cáo của người dùng hiện tại.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại, bắt đầu từ 1.</param>
    /// <param name="pageSize">Số bản ghi trên mỗi trang.</param>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyReports(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var reports = await _service.GetMyReportsAsync(pageIndex, pageSize);
            return Ok(SuccessResponse<object>.Create(reports, "Lấy danh sách report của tôi thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách report của tôi thất bại.");
            throw;
        }
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

    /// <summary>
    /// Cập nhật báo cáo của người dùng hiện tại.
    /// </summary>
    /// <param name="reportId">Mã báo cáo.</param>
    /// <param name="request">Nội dung báo cáo sau khi cập nhật.</param>
    [HttpPut("{reportId:guid}")]
    [SwaggerRequestExample(typeof(UpdateReportRequestDTO), typeof(UpdateReportRequestExample))]
    public async Task<IActionResult> UpdateMyReport(Guid reportId, [FromBody] UpdateReportRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateMyReportAsync(reportId, request);
            return Ok(SuccessResponse<ReportResponseDTO>.Create(updated, "Cập nhật report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật report thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Phản hồi báo cáo.
    /// </summary>
    /// <param name="reportId">Mã báo cáo.</param>
    /// <param name="request">Nội dung phản hồi.</param>
    [HttpPatch("{reportId:guid}/response")]
    [Authorize(Roles = Roles.AdminOrManagerRoles)]
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

    /// <summary>
    /// Xóa báo cáo của người dùng hiện tại.
    /// </summary>
    /// <param name="reportId">Mã báo cáo.</param>
    [HttpDelete("{reportId:guid}")]
    public async Task<IActionResult> DeleteMyReport(Guid reportId)
    {
        try
        {
            await _service.DeleteMyReportAsync(reportId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa report thất bại.");
            throw;
        }
    }
}
