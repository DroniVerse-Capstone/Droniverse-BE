using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Domain.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<PaginationResult<IEnumerable<PermissionResponse>>> GetAllPermissionsAsync(
        IPermissionSearchSpecification spec,
        int pageIndex,
        int pageSize);
}

