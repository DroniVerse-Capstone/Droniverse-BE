using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.API.Enums;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.API.Validators;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/drones")]
[ApiController]
public class DroneController : ControllerBase
{
    private readonly ILogger<DroneController> _logger;
    private readonly IDroneService _droneService;
    private readonly IDroneTypeService _droneTypeService;
    private readonly IRequiredDroneService _requiredDroneService;

    public DroneController(ILogger<DroneController> logger, IDroneService droneService, IDroneTypeService droneTypeService, IRequiredDroneService requiredDroneService)
    {
        _logger = logger;
        _droneService = droneService;
        _droneTypeService = droneTypeService;
        _requiredDroneService = requiredDroneService;
    }

    /// <summary>
    /// Tạo mới một drone.
    /// </summary>
    /// <param name="droneTypeId">Mã loại drone.</param>
    /// <param name="request">Thông tin drone cần tạo.</param>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerRequestExample(typeof(CreateDroneRequestDTO), typeof(CreateDroneRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateDrone([FromQuery] Guid droneTypeId, [FromBody] CreateDroneRequestDTO request)
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

    /// <summary>
    /// Lấy danh sách drone và lọc theo trạng thái.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetDrones([FromQuery] DroneStatusFilter status = DroneStatusFilter.All)
    {
        try
        {
            var drones = await _droneService.GetDronesAsync(MapDroneStatus(status));
            return Ok(SuccessResponse<IEnumerable<DroneClientViewDTO>>.Create(drones, "Lấy danh sách drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách drone thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết một drone.
    /// </summary>
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
            _logger.LogError(ex, "Lấy chi tiết drone thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật thông tin drone.
    /// </summary>
    /// <param name="droneId">Mã drone.</param>
    /// <param name="request">Thông tin drone cần cập nhật.</param>
    [HttpPut("{droneId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerRequestExample(typeof(UpdateDroneRequestDTO), typeof(UpdateDroneRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateDrone(Guid droneId, [FromBody] UpdateDroneRequestDTO request)
    {
        try
        {
            var updated = await _droneService.UpdateDroneAsync(droneId, request);
            return Ok(SuccessResponse<DroneClientViewDTO>.Create(updated, "Cập nhật drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật drone thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa một drone.
    /// </summary>
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
            _logger.LogError(ex, "Xóa drone thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách phiên bản khóa học theo drone.
    /// </summary>
    [HttpGet("{droneId:guid}/course-versions")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetCourseVersionsByDrone(Guid droneId)
    {
        try
        {
            RequiredDroneControllerValidator.ValidateGetCourseVersionsByDrone(droneId);
            var courseVersions = await _requiredDroneService.GetCourseVersionsByDroneAsync(droneId);
            return Ok(SuccessResponse<IEnumerable<CourseVersionByDroneClientViewDTO>>.Create(courseVersions, "Lấy danh sách course version theo drone thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách phiên bản khóa học theo drone thất bại.");
            throw;
        }
    }

    private static DroneStatus? MapDroneStatus(DroneStatusFilter status)
    {
        return status switch
        {
            DroneStatusFilter.All => null,
            DroneStatusFilter.Draft => DroneStatus.DRAFT,
            DroneStatusFilter.Available => DroneStatus.AVAILABLE,
            DroneStatusFilter.Maintenance => DroneStatus.MAINTENANCE,
            _ => null
        };
    }
}
