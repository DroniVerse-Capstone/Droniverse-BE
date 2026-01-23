using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Identity.Domain.Entities;

namespace Droniverse.Identity.Application.IService;
public interface IRoleService
{
    Task<RoleResponse> AddRole(RoleCreateDto role);
    Task<RoleResponse> UpdateRole(Guid roleId, RoleUpdateDto role);
    Task<bool> DeleteRole(Guid id);
    Task<IEnumerable<RoleResponse>> GetAllRoles();
    Task<RoleResponse> GetRoleById(Guid id);
}

