using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/vr-simulators")]
[ApiController]
public class VRSimulatorController : ControllerBase
{
    private readonly ILogger<VRSimulatorController> _logger;
    private readonly IVRSimulatorService _vrSimulatorService;

    public VRSimulatorController(ILogger<VRSimulatorController> logger, IVRSimulatorService vrSimulatorService)
    {
        _logger = logger;
        _vrSimulatorService = vrSimulatorService;
    }

    /// <summary>
    /// Tạo mới vr simulator.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateVRSimulator([FromBody] CreateVRSimulatorRequestDTO request)
    {
        try
        {
            var created = await _vrSimulatorService.CreateVRSimulatorAsync(request);
            return StatusCode(201, SuccessResponse<VRSimulatorClientViewDTO>.Create(created, "Tạo vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo vr simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Tạo lesson từ vr simulator có sẵn.
    /// </summary>
    [HttpPost("{vrSimulatorId:guid}/lessons")]
    public async Task<IActionResult> CreateLessonFromVRSimulator(Guid vrSimulatorId, [FromBody] CreateVRSimulatorLessonRequestDTO request)
    {
        try
        {
            var created = await _vrSimulatorService.CreateLessonFromVRSimulatorAsync(vrSimulatorId, request);
            return StatusCode(201, SuccessResponse<LessonClientViewDTO>.Create(created, "Tạo lesson từ vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo lesson từ vr simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách vr simulator.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetVRSimulators(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] VRSimulatorType? type = null)
    {
        try
        {
            var vrSimulators = await _vrSimulatorService.GetVRSimulatorsAsync(pageIndex, pageSize, search, type);
            return Ok(SuccessResponse<PaginationResult<IEnumerable<VRSimulatorClientViewDTO>>>.Create(vrSimulators, "Lấy danh sách vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách vr simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết vr simulator.
    /// </summary>
    [HttpGet("{vrSimulatorId:guid}")]
    public async Task<IActionResult> GetVRSimulatorById(Guid vrSimulatorId)
    {
        try
        {
            var vrSimulator = await _vrSimulatorService.GetVRSimulatorByIdAsync(vrSimulatorId);
            return Ok(SuccessResponse<VRSimulatorClientViewDTO>.Create(vrSimulator, "Lấy chi tiết vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết vr simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật vr simulator.
    /// </summary>
    [HttpPut("{vrSimulatorId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateVRSimulator(Guid vrSimulatorId, [FromBody] UpdateVRSimulatorRequestDTO request)
    {
        try
        {
            var updated = await _vrSimulatorService.UpdateVRSimulatorAsync(vrSimulatorId, request);
            return Ok(SuccessResponse<VRSimulatorClientViewDTO>.Create(updated, "Cập nhật vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật vr simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa vr simulator.
    /// </summary>
    [HttpDelete("{vrSimulatorId:guid}")]
    public async Task<IActionResult> DeleteVRSimulator(Guid vrSimulatorId)
    {
        try
        {
            await _vrSimulatorService.DeleteVRSimulatorAsync(vrSimulatorId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa vr simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa vr simulator thất bại.");
            throw;
        }
    }

    [HttpGet("{vrSimulatorId:Guid}/check")]
    public async Task<SimpleVRSimulatorResponse> GetSimpleVRSimulator(Guid
         vrSimulatorId)
    {
        var result = await _vrSimulatorService.GetSimpleVRSimulator(vrSimulatorId);
        return result;
    }

    [HttpPost("check")]
    public async Task<IEnumerable<SimpleVRSimulatorResponse>> GetVRSimulatorByIds([FromBody] IEnumerable<Guid> vrSimulatorIds)
    {
        var result = await _vrSimulatorService.GetVRSimulatorByIds(vrSimulatorIds);
        return result;
    }
}
