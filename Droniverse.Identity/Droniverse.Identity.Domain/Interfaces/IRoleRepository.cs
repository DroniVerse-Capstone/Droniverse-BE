using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Identity.Domain.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<PaginationResult<IEnumerable<RoleResponse>>> GetAllRolesAsync(
        IRoleSearchSpecification spec,
        int pageIndex,
        int pageSize);
}

