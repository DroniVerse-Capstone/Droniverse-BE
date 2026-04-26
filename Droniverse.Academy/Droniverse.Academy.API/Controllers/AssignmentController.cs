using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

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

    /// <summary>
    /// Tạo mới một assignment cho lesson kiểu assignment.
    /// </summary>
    /// <param name="request">Thông tin assignment cần tạo.</param>
    /// <returns>Assignment vừa được tạo.</returns>
    [HttpPost]
    [SwaggerRequestExample(typeof(CreateAssignmentRequestDTO), typeof(CreateAssignmentRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status201Created, typeof(AssignmentSuccessResponseExample))]
    [ProducesResponseType(typeof(SuccessResponse<AssignmentClientViewDTO>), StatusCodes.Status201Created)]
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

    /// <summary>
    /// Lấy danh sách assignment có phân trang.
    /// </summary>
    /// <param name="pageIndex">Trang hiện tại.</param>
    /// <param name="pageSize">Số lượng phần tử trên một trang.</param>
    /// <returns>Danh sách assignment theo trang.</returns>
    [HttpGet]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AssignmentListSuccessResponseExample))]
    [ProducesResponseType(typeof(SuccessResponse<PaginationResult<IEnumerable<AssignmentClientViewDTO>>>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Lấy chi tiết một assignment theo ID.
    /// </summary>
    /// <param name="assignmentId">ID của assignment.</param>
    /// <returns>Thông tin assignment.</returns>
    [HttpGet("{assignmentId:guid}")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AssignmentSuccessResponseExample))]
    [ProducesResponseType(typeof(SuccessResponse<AssignmentClientViewDTO>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Cập nhật thông tin assignment.
    /// </summary>
    /// <param name="assignmentId">ID của assignment cần cập nhật.</param>
    /// <param name="request">Thông tin cập nhật.</param>
    /// <returns>Assignment sau khi cập nhật.</returns>
    [HttpPut("{assignmentId:guid}")]
    [SwaggerRequestExample(typeof(UpdateAssignmentRequestDTO), typeof(UpdateAssignmentRequestExample))]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AssignmentSuccessResponseExample))]
    [ProducesResponseType(typeof(SuccessResponse<AssignmentClientViewDTO>), StatusCodes.Status200OK)]
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

    /// <summary>
    /// Xóa một assignment.
    /// </summary>
    /// <param name="assignmentId">ID của assignment cần xóa.</param>
    /// <returns>Không có nội dung nếu xóa thành công.</returns>
    [HttpDelete("{assignmentId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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
