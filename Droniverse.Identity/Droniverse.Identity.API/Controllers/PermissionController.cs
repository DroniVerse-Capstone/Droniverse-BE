using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Application.IService;
using Microsoft.AspNetCore.Mvc;

namespace Droniverse.Identity.API.Controllers
{
    [Route("identity/permissions")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionService.GetAllPermissions();
            return Ok(permissions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPermissionById(Guid id)
        {
            var permission = await _permissionService.GetPermissionById(id);
            return Ok(permission);
        }

        [HttpPost]
        public async Task<IActionResult> AddPermission([FromBody] PermissionCreateDto permissionCreateDto)
        {
            PermissionResponse permission = await _permissionService.AddPermission(permissionCreateDto);
            return CreatedAtAction(nameof(GetPermissionById), new { id = permission.PermissionId }, permission);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission(Guid id, [FromBody] PermissionUpdateDto permissionUpdateDto)
        {
            PermissionResponse updatedPermission = await _permissionService.UpdatePermission(id, permissionUpdateDto);
            return Ok(updatedPermission);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(Guid id)
        {
            bool isDeleted = await _permissionService.DeletePermission(id);
            if(isDeleted)
                return NoContent();
            return BadRequest("Failed to delete the permission.");
        }
    }
}
