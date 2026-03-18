using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/drone-types")]
[ApiController]
public class DroneTypeController : ControllerBase
{
    private readonly ILogger<DroneTypeController> _logger;
    private readonly IDroneTypeService _droneTypeService;

    public DroneTypeController(ILogger<DroneTypeController> logger, IDroneTypeService droneTypeService)
    {
        _logger = logger;
        _droneTypeService = droneTypeService;
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateDroneType([FromBody] CreateDroneTypeRequestDTO request)
    {
        try
        {
            var created = await _droneTypeService.CreateDroneTypeAsync(request);
            return StatusCode(201, SuccessResponse<DroneTypeClientViewDTO>.Create(created, "Tạo loại drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo loại drone thất bại.");
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDroneTypes()
    {
        try
        {
            var types = await _droneTypeService.GetDroneTypesAsync();
            return Ok(SuccessResponse<IEnumerable<DroneTypeClientViewDTO>>.Create(types, "Lấy danh sách loại drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách loại drone thất bại.");
            throw;
        }
    }

    [HttpGet("{droneTypeId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDroneTypeById(Guid droneTypeId)
    {
        try
        {
            var type = await _droneTypeService.GetDroneTypeByIdAsync(droneTypeId);
            return Ok(SuccessResponse<DroneTypeClientViewDTO>.Create(type, "Lấy chi tiết loại drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết loại drone thất bại.");
            throw;
        }
    }

    [HttpPut("{droneTypeId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateDroneType(Guid droneTypeId, [FromBody] UpdateDroneTypeRequestDTO request)
    {
        try
        {
            var updated = await _droneTypeService.UpdateDroneTypeAsync(droneTypeId, request);
            return Ok(SuccessResponse<DroneTypeClientViewDTO>.Create(updated, "Cập nhật loại drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật loại drone thất bại.");
            throw;
        }
    }

    [HttpDelete("{droneTypeId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteDroneType(Guid droneTypeId)
    {
        try
        {
            await _droneTypeService.DeleteDroneTypeAsync(droneTypeId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa loại drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa loại drone thất bại.");
            throw;
        }
    }

    [HttpPost("{droneTypeId:guid}/drones")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> CreateDrone(Guid droneTypeId, [FromBody] CreateDroneRequestDTO request)
    {
        try
        {
            var created = await _droneTypeService.CreateDroneAsync(droneTypeId, request);
            return StatusCode(201, SuccessResponse<DroneClientViewDTO>.Create(created, "Tạo drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo drone thất bại.");
            throw;
        }
    }

    [HttpGet("{droneTypeId:guid}/drones")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDronesByType(Guid droneTypeId)
    {
        try
        {
            var drones = await _droneTypeService.GetDronesByTypeAsync(droneTypeId);
            return Ok(SuccessResponse<IEnumerable<DroneClientViewDTO>>.Create(drones, "Lấy danh sách drone theo loại thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách drone theo loại thất bại.");
            throw;
        }
    }
}
