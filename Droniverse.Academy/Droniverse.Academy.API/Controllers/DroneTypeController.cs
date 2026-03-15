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
            return StatusCode(201, SuccessResponse<DroneTypeClientViewDTO>.Create(created, "T?o lo?i drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateDroneType failed");
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
            return Ok(SuccessResponse<IEnumerable<DroneTypeClientViewDTO>>.Create(types, "L?y danh sách lo?i drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDroneTypes failed");
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
            return Ok(SuccessResponse<DroneTypeClientViewDTO>.Create(type, "L?y chi ti?t lo?i drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDroneTypeById failed for {DroneTypeId}", droneTypeId);
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
            return Ok(SuccessResponse<DroneTypeClientViewDTO>.Create(updated, "C?p nh?t lo?i drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateDroneType failed for {DroneTypeId}", droneTypeId);
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
            return Ok(SuccessResponse<object>.Create(null!, "Xóa lo?i drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDroneType failed for {DroneTypeId}", droneTypeId);
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
            return StatusCode(201, SuccessResponse<DroneClientViewDTO>.Create(created, "T?o drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateDrone failed for {DroneTypeId}", droneTypeId);
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
            return Ok(SuccessResponse<IEnumerable<DroneClientViewDTO>>.Create(drones, "L?y danh sách drone theo lo?i thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDronesByType failed for {DroneTypeId}", droneTypeId);
            throw;
        }
    }
}
