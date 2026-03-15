using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Shared.Constants;
using Droniverse.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> CreateModule(Guid courseId, Guid versionId, [FromBody] CreateModuleRequestDTO request)
    {
        try
        {
            var created = await _service.CreateModuleAsync(courseId, versionId, request);
            return StatusCode(201,
                SuccessResponse<ModuleClientViewDTO>.Create(created, "T?o module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateModule failed for {CourseId}/{VersionId}", courseId, versionId);
            throw;
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetModules(Guid courseId, Guid versionId)
    {
        try
        {
            var result = await _service.GetModulesAsync(courseId, versionId);
            return Ok(SuccessResponse<IEnumerable<ModuleClientViewDTO>>.Create(result, "L?y danh sách module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetModules failed for {CourseId}/{VersionId}", courseId, versionId);
            throw;
        }
    }

    [HttpGet("{moduleId:guid}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> GetModuleById(Guid courseId, Guid versionId, Guid moduleId)
    {
        try
        {
            var result = await _service.GetModuleByIdAsync(courseId, versionId, moduleId);
            return Ok(SuccessResponse<ModuleClientViewDTO>.Create(result, "L?y chi ti?t module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetModuleById failed for {CourseId}/{VersionId}/{ModuleId}", courseId, versionId, moduleId);
            throw;
        }
    }

    [HttpPut("{moduleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateModule(Guid courseId, Guid versionId, Guid moduleId, [FromBody] UpdateModuleRequestDTO request)
    {
        try
        {
            var updated = await _service.UpdateModuleAsync(courseId, versionId, moduleId, request);
            return Ok(SuccessResponse<ModuleClientViewDTO>.Create(updated, "C?p nh?t module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateModule failed for {CourseId}/{VersionId}/{ModuleId}", courseId, versionId, moduleId);
            throw;
        }
    }

    [HttpDelete("{moduleId:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> DeleteModule(Guid courseId, Guid versionId, Guid moduleId)
    {
        try
        {
            await _service.DeleteModuleAsync(courseId, versionId, moduleId);
            return Ok(SuccessResponse<object>.Create(null!, "Xóa module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeleteModule failed for {CourseId}/{VersionId}/{ModuleId}", courseId, versionId, moduleId);
            throw;
        }
    }

    [HttpPatch("reorder")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ReorderModules(Guid courseId, Guid versionId, [FromBody] ReorderModulesRequestDTO request)
    {
        try
        {
            var result = await _service.ReorderModulesAsync(courseId, versionId, request);
            return Ok(SuccessResponse<IEnumerable<ModuleClientViewDTO>>.Create(result, "S?p x?p l?i module thành công."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ReorderModules failed for {CourseId}/{VersionId}", courseId, versionId);
            throw;
        }
    }

}
