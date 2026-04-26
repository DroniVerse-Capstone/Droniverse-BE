using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/assignments")]
[ApiController]
[Authorize(Roles = Roles.AdminOrSystemManager)]
public class AssignmentController : ControllerBase
{
    private readonly ILogger<AssignmentController> _logger;
    private readonly IAssignmentService _service;

    public AssignmentController(ILogger<AssignmentController> logger, IAssignmentService service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentRequestDTO request)
    {
        try
        {
            var created = await _service.CreateAssignmentAsync(request);
            return StatusCode(201, SuccessResponse<AssignmentClientViewDTO>.Create(created, "Tạo assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo assignment thất bại.");
            throw;
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAssignments([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _service.GetAssignmentsAsync(pageIndex, pageSize);
            return Ok(SuccessResponse<PaginationResult<IEnumerable<AssignmentClientViewDTO>>>.Create(result, "Lấy danh sách assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách assignment thất bại.");
            throw;
        }
    }

    [HttpGet("{assignmentId:guid}")]
    public async Task<IActionResult> GetAssignmentById(Guid assignmentId)
    {
        try
        {
            var result = await _service.GetAssignmentByIdAsync(assignmentId);
            return Ok(SuccessResponse<AssignmentClientViewDTO>.Create(result, "Lấy chi tiết assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết assignment thất bại.");
            throw;
        }
    }

    [HttpPut("{assignmentId:guid}")]
    public async Task<IActionResult> UpdateAssignment(Guid assignmentId, [FromBody] UpdateAssignmentRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateAssignmentAsync(assignmentId, request);
            return Ok(SuccessResponse<AssignmentClientViewDTO>.Create(updated, "Cập nhật assignment thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật assignment thất bại.");
            throw;
        }
    }

    [HttpDelete("{assignmentId:guid}")]
    public async Task<IActionResult> DeleteAssignment(Guid assignmentId)
    {
        try
        {
            await _service.DeleteAssignmentAsync(assignmentId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa assignment thất bại.");
            throw;
        }
    }
}
