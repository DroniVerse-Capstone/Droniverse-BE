using Droniverse.Identity.Application.DTO.Request;
using Droniverse.Identity.Application.DTO.Response;

namespace Droniverse.Identity.Application.IService;
public interface IPermissionService
{
    Task<IEnumerable<PermissionResponse>> GetAllPermissions();
    Task<PermissionResponse> GetPermissionById(Guid id);
    Task<PermissionResponse> AddPermission(PermissionCreateDto permissionCreateDto);
    Task<bool> DeletePermission(Guid id);
    Task<PermissionResponse> UpdatePermission(Guid id, PermissionUpdateDto permissionUpdateDto);
}

