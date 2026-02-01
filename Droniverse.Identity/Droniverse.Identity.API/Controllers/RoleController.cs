using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Droniverse.Identity.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [Route("identity/roles")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
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
            return CreatedAtAction(nameof(GetRoleById), new {id = r.RoleId}, r);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            IEnumerable<RoleResponse> roles = await _roleService.GetAllRoles();
            return Ok(roles);
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
