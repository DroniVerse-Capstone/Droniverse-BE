using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Academy.API.Validators;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/versions/{versionId:guid}/required-drones")]
[ApiController]
public class CourseVersionRequiredDroneController : ControllerBase
{
    private readonly ILogger<CourseVersionRequiredDroneController> _logger;
    private readonly IRequiredDroneService _requiredDroneService;

    public CourseVersionRequiredDroneController(ILogger<CourseVersionRequiredDroneController> logger, IRequiredDroneService requiredDroneService)
    {
        _logger = logger;
        _requiredDroneService = requiredDroneService;
    }

    /// <summary>
    /// Gán drone yêu cầu cho phiên bản khóa học.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="request">Thông tin drone cần gán cho phiên bản khóa học.</param>
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(AddRequiredDroneRequestDTO), typeof(AddRequiredDroneRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddRequiredDrone(Guid courseId, Guid versionId, [FromBody] AddRequiredDroneRequestDTO request)
    {
        try
        {
            RequiredDroneControllerValidator.ValidateAddRequiredDrone(courseId, versionId, request);
            var added = await _requiredDroneService.AddRequiredDroneAsync(courseId, versionId, request);
            return StatusCode(201, SuccessResponse<DroneClientViewDTO>.Create(added, "Gán drone yêu cầu thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gán drone yêu cầu thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Gỡ drone yêu cầu khỏi phiên bản khóa học.
    /// </summary>
    [HttpDelete("{droneId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> RemoveRequiredDrone(Guid courseId, Guid versionId, Guid droneId)
    {
        try
        {
            RequiredDroneControllerValidator.ValidateRemoveRequiredDrone(courseId, versionId, droneId);
            await _requiredDroneService.RemoveRequiredDroneAsync(courseId, versionId, droneId);
            return Ok(SuccessResponse<object>.Create(null!, "Gỡ drone yêu cầu thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gỡ drone yêu cầu thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách drone yêu cầu của phiên bản khóa học.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetRequiredDrones(Guid courseId, Guid versionId)
    {
        try
        {
            RequiredDroneControllerValidator.ValidateGetRequiredDrones(courseId, versionId);
            var drones = await _requiredDroneService.GetRequiredDronesAsync(courseId, versionId);
            return Ok(SuccessResponse<IEnumerable<DroneClientViewDTO>>.Create(drones, "Lấy danh sách drone yêu cầu thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách drone yêu cầu thất bại.");
            throw;
        }
    }
}
