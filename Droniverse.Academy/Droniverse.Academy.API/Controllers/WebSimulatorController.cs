using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/web-simulators")]
[ApiController]
public class WebSimulatorController : ControllerBase
{
    private readonly ILogger<WebSimulatorController> _logger;
    private readonly IWebSimulatorService _webSimulatorService;

    public WebSimulatorController(ILogger<WebSimulatorController> logger, IWebSimulatorService webSimulatorService)
    {
        _logger = logger;
        _webSimulatorService = webSimulatorService;
    }

    /// <summary>
    /// Tạo mới web simulator.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateWebSimulator([FromBody] CreateWebSimulatorRequestDTO request)
    {
        try
        {
            var created = await _webSimulatorService.CreateWebSimulatorAsync(request);
            return StatusCode(201, SuccessResponse<WebSimulatorClientViewDTO>.Create(created, "Tạo web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo web simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Tạo lesson từ web simulator có sẵn.
    /// </summary>
    [HttpPost("{webSimulatorId:guid}/lessons")]
    public async Task<IActionResult> CreateLessonFromWebSimulator(Guid webSimulatorId, [FromBody] CreateWebSimulatorLessonRequestDTO request)
    {
        try
        {
            var created = await _webSimulatorService.CreateLessonFromWebSimulatorAsync(webSimulatorId, request);
            return StatusCode(201, SuccessResponse<LessonClientViewDTO>.Create(created, "Tạo lesson từ web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo lesson từ web simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách web simulator.
    /// </summary>
    /// <param name="type">Lọc theo loại web simulator.</param>
    /// <param name="droneId">Lọc theo droneID của web simulator.</param>
    [HttpGet]
    public async Task<IActionResult> GetWebSimulators([FromQuery] WebSimulatorType? type = null, [FromQuery] Guid? droneId = null)
    {
        try
        {
            var webSimulators = await _webSimulatorService.GetWebSimulatorsAsync(type, droneId);
            return Ok(SuccessResponse<IEnumerable<WebSimulatorClientViewDTO>>.Create(webSimulators, "Lấy danh sách web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách web simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết web simulator.
    /// </summary>
    [HttpGet("{webSimulatorId:guid}")]
    public async Task<IActionResult> GetWebSimulatorById(Guid webSimulatorId)
    {
        try
        {
            var webSimulator = await _webSimulatorService.GetWebSimulatorByIdAsync(webSimulatorId);
            return Ok(SuccessResponse<WebSimulatorClientViewDTO>.Create(webSimulator, "Lấy chi tiết web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết web simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật web simulator.
    /// </summary>
    [HttpPut("{webSimulatorId:guid}")]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateWebSimulator(Guid webSimulatorId, [FromBody] UpdateWebSimulatorRequestDTO request)
    {
        try
        {
            var updated = await _webSimulatorService.UpdateWebSimulatorAsync(webSimulatorId, request);
            return Ok(SuccessResponse<WebSimulatorClientViewDTO>.Create(updated, "Cập nhật web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật web simulator thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa web simulator.
    /// </summary>
    [HttpDelete("{webSimulatorId:guid}")]
    public async Task<IActionResult> DeleteWebSimulator(Guid webSimulatorId)
    {
        try
        {
            await _webSimulatorService.DeleteWebSimulatorAsync(webSimulatorId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa web simulator thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa web simulator thất bại.");
            throw;
        }
    }
}
