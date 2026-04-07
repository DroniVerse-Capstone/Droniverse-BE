using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Application.IService;

public interface IPermissionService
{
    Task<PaginationResult<IEnumerable<PermissionResponse>>> GetAllPermissions(
        PermissionSearchRequest permissionSearchRequest,
        int pageIndex,
        int pageSize);

    Task<PermissionResponse> GetPermissionById(Guid id);
    
    Task<PermissionResponse> AddPermission(PermissionCreateDto permissionCreateDto);
    
    Task<bool> DeletePermission(Guid id);
    
    Task<PermissionResponse> UpdatePermission(Guid id, PermissionUpdateDto permissionUpdateDto);
}

