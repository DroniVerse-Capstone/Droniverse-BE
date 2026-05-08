using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Application.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Droniverse.Shared.Constants;

namespace Droniverse.Identity.API.Controllers;

[ApiController]
[Route("identity/system-policies")]
public class SysPolicyController : ControllerBase
{
    private readonly ISysPolicyService _sysPolicyService;

    public SysPolicyController(ISysPolicyService sysPolicyService)
    {
        _sysPolicyService = sysPolicyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SysPolicySearchRequest searchRequest)
    {
        searchRequest ??= new SysPolicySearchRequest();
        var result = await _sysPolicyService.GetAllSysPolicies(searchRequest, searchRequest.CurrentPage, searchRequest.PageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.AllRoles)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var policy = await _sysPolicyService.GetSysPolicyById(id);
        return Ok(policy);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> Create(SysPolicyCreateDto dto)
    {
        var created = await _sysPolicyService.AddSysPolicy(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.SysPolicyID }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> Update(Guid id, SysPolicyUpdateDto dto)
    {
        var updated = await _sysPolicyService.UpdateSysPolicy(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.AdminOrSystemManager)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _sysPolicyService.DeleteSysPolicy(id);
        if (deleted)
            return NoContent();
        return BadRequest("Failed to delete sys policy.");
    }
}
