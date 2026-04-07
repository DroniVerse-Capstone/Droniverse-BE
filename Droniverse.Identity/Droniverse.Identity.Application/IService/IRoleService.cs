using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.IService;

public interface IRoleService
{
    Task<PaginationResult<IEnumerable<RoleResponse>>> GetAllRoles(
        RoleSearchRequest roleSearchRequest,
        int pageIndex,
        int pageSize);

    Task<RoleResponse> AddRole(RoleCreateDto role);
    
    Task<RoleResponse> UpdateRole(Guid roleId, RoleUpdateDto role);
    
    Task<bool> DeleteRole(Guid id);
    
    Task<RoleResponse> GetRoleById(Guid id);
}

