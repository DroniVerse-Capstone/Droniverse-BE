using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Droniverse.Identity.Infrastructure.Repositories;

internal class SysPolicyRepository : Repository<SysPolicy>, ISysPolicyRepository
{
    public SysPolicyRepository(IdentityDbContext context) : base(context)
    {
    }

    public async Task<PaginationResult<IEnumerable<SysPolicy>>> GetAllSysPoliciesAsync(
        ISysPolicySearchSpecification spec,
        int pageIndex,
        int pageSize)
    {
        IQueryable<SysPolicy> query = _dbSet.AsNoTracking();

        if (spec?.Type.HasValue == true)
        {
            query = query.Where(s => s.Type == spec.Type.Value);
        }

        if (!string.IsNullOrWhiteSpace(spec?.Title))
        {
            var term = spec.Title.Trim();
            query = query.Where(s => s.Title.Contains(term));
        }

        query = query.OrderByDescending(s => s.EffectiveDate);

        var totalRecords = await query.CountAsync();
        var skip = (pageIndex - 1) * pageSize;
        var items = await query
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<SysPolicy>>(items, totalRecords, pageIndex, pageSize);
    }
}
