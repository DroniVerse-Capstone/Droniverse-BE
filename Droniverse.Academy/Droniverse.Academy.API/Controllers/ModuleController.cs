using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.API.Examples;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

namespace Droniverse.Academy.API.Controllers;

[Route("academy/courses/{courseId:guid}/versions/{versionId:guid}/modules")]
[ApiController]
public class ModuleController : ControllerBase
{
    private readonly ILogger<ModuleController> _logger;
    private readonly IModuleService _service;

    public ModuleController(ILogger<ModuleController> logger, IModuleService service)
    {
        _logger = logger;
        _service = service;
    }

    /// <summary>
    /// Tạo mô-đun mới.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="request">Thông tin mô-đun cần tạo.</param>
    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    [SwaggerRequestExample(typeof(CreateModuleRequestDTO), typeof(CreateModuleRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateModule(Guid courseId, Guid versionId, [FromBody] CreateModuleRequestDTO request)
    {
        try
        {
            var created = await _service.CreateModuleAsync(courseId, versionId, request);
            return StatusCode(201,
                SuccessResponse<ModuleClientViewDTO>.Create(created, "Tạo mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Tạo mô-đun thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy danh sách mô-đun.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetModules(Guid courseId, Guid versionId)
    {
        try
        {
            var result = await _service.GetModulesAsync(courseId, versionId);
            return Ok(SuccessResponse<IEnumerable<ModuleClientViewDTO>>.Create(result, "Lấy danh sách mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy danh sách mô-đun thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Lấy chi tiết mô-đun.
    /// </summary>
    [HttpGet("{moduleId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetModuleById(Guid courseId, Guid versionId, Guid moduleId)
    {
        try
        {
            var result = await _service.GetModuleByIdAsync(courseId, versionId, moduleId);
            return Ok(SuccessResponse<ModuleClientViewDTO>.Create(result, "Lấy chi tiết mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lấy chi tiết mô-đun thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Cập nhật mô-đun.
    /// </summary>
    /// <param name="courseId">Mã khóa học.</param>
    /// <param name="versionId">Mã phiên bản khóa học.</param>
    /// <param name="moduleId">Mã mô-đun.</param>
    /// <param name="request">Thông tin mô-đun cần cập nhật.</param>
    [HttpPut("{moduleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    [SwaggerRequestExample(typeof(UpdateModuleRequestDTO), typeof(UpdateModuleRequestExample))]
    [ProducesResponseType(typeof(SuccessResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateModule(Guid courseId, Guid versionId, Guid moduleId, [FromBody] UpdateModuleRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateModuleAsync(courseId, versionId, moduleId, request);
            return Ok(SuccessResponse<ModuleClientViewDTO>.Create(updated, "Cập nhật mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cập nhật mô-đun thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Xóa mô-đun.
    /// </summary>
    [HttpDelete("{moduleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteModule(Guid courseId, Guid versionId, Guid moduleId)
    {
        try
        {
            await _service.DeleteModuleAsync(courseId, versionId, moduleId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Xóa mô-đun thất bại.");
            throw;
        }
    }

    /// <summary>
    /// Sắp xếp lại thứ tự mô-đun.
    /// </summary>
    [HttpPatch("reorder")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ReorderModules(Guid courseId, Guid versionId, [FromBody] ReorderModulesRequestDTO request)
    {
        try
        {
            var result = await _service.ReorderModulesAsync(courseId, versionId, request);
            return Ok(SuccessResponse<IEnumerable<ModuleClientViewDTO>>.Create(result, "Sắp xếp lại mô-đun thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Sắp xếp lại mô-đun thất bại.");
            throw;
        }
    }

}
