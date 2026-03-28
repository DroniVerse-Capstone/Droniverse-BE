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

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrManagerRoles)]
    public async Task<IActionResult> GetReports(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? labId = null,
        [FromQuery] Guid? userId = null)
    {
        try
        {
            var reports = await _service.GetReportsAsync(pageIndex, pageSize, labId, userId);
            return Ok(SuccessResponse<object>.Create(reports, "Lấy danh sách report thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách report thất bại.");
            throw;
        }
    }

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
