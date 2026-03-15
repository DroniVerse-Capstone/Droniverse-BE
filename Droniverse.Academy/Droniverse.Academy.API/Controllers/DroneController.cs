using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/drones")]
[ApiController]
public class DroneController : ControllerBase
{
    private readonly ILogger<DroneController> _logger;
    private readonly IDroneService _droneService;
    private readonly IRequiredDroneService _requiredDroneService;

    public DroneController(ILogger<DroneController> logger, IDroneService droneService, IRequiredDroneService requiredDroneService)
    {
        _logger = logger;
        _droneService = droneService;
        _requiredDroneService = requiredDroneService;
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDrones()
    {
        try
        {
            var drones = await _droneService.GetDronesAsync();
            return Ok(SuccessResponse<IEnumerable<DroneClientViewDTO>>.Create(drones, "Lấy danh sách drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDrones failed");
            throw;
        }
    }

    [HttpGet("{droneId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDroneById(Guid droneId)
    {
        try
        {
            var drone = await _droneService.GetDroneByIdAsync(droneId);
            return Ok(SuccessResponse<DroneClientViewDTO>.Create(drone, "Lấy chi tiết drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDroneById failed for {DroneId}", droneId);
            throw;
        }
    }

    [HttpPut("{droneId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateDrone(Guid droneId, [FromBody] UpdateDroneRequestDTO request)
    {
        try
        {
            var updated = await _droneService.UpdateDroneAsync(droneId, request);
            return Ok(SuccessResponse<DroneClientViewDTO>.Create(updated, "C?p nh?t drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateDrone failed for {DroneId}", droneId);
            throw;
        }
    }

    [HttpDelete("{droneId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteDrone(Guid droneId)
    {
        try
        {
            await _droneService.DeleteDroneAsync(droneId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteDrone failed for {DroneId}", droneId);
            throw;
        }
    }

    [HttpGet("{droneId:guid}/course-versions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetCourseVersionsByDrone(Guid droneId)
    {
        try
        {
            var courseVersions = await _requiredDroneService.GetCourseVersionsByDroneAsync(droneId);
            return Ok(SuccessResponse<IEnumerable<CourseVersionByDroneClientViewDTO>>.Create(courseVersions, "Lấy danh sách course version theo drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCourseVersionsByDrone failed for {DroneId}", droneId);
            throw;
        }
    }
}
