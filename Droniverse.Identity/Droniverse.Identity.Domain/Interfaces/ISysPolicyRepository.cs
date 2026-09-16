using Droniverse.Identity.Domain.Entities;
using Droniverse.Shared.DTOs.Response;
using System.Collections.Generic;

namespace Droniverse.Identity.Domain.Interfaces;

public interface ISysPolicyRepository : IRepository<SysPolicy>
{
    Task<PaginationResult<IEnumerable<SysPolicy>>> GetAllSysPoliciesAsync(
        ISysPolicySearchSpecification spec,
        int pageIndex,
        int pageSize);
}
