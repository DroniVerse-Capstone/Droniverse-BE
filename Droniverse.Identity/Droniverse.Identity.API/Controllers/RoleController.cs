using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.IService;
using Droniverse.Shared.DTOs.Response;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [ApiController]
    [Route("identity/roles")]
    [Authorize]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles(
            [FromQuery] RoleSearchRequest roleSearchRequest)
        {
            roleSearchRequest ??= new RoleSearchRequest();
            var roles = await _roleService.GetAllRoles(
                roleSearchRequest,
                roleSearchRequest.CurrentPage,
                roleSearchRequest.PageSize);
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            RoleResponse role = await _roleService.GetRoleById(id);
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(RoleCreateDto roleCreateDto)
        {
            RoleResponse r = await _roleService.AddRole(roleCreateDto);
            return CreatedAtAction(nameof(GetRoleById), new { id = r.RoleId }, r);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(Guid id, RoleUpdateDto roleUpdateDto)
        {
            RoleResponse updatedRole = await _roleService.UpdateRole(id, roleUpdateDto);
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            bool result = await _roleService.DeleteRole(id);
            if (result)
            {
                return NoContent();
            }
            return NotFound();
        }
    }
}
