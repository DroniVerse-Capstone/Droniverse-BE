using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Identity.Infrastructure.Repositories;

internal class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(IdentityDbContext context) : base(context)
    {
    }

    public new async Task<IEnumerable<Role>> GetAll()
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .ToListAsync();
    }

    public new async Task<Role?> GetByCondition(Expression<Func<Role, bool>> expression)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .Where(expression)
            .FirstOrDefaultAsync();
    }

    public async Task<PaginationResult<IEnumerable<RoleResponse>>> GetAllRolesAsync(
        IRoleSearchSpecification spec,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Role> query = _dbSet.AsNoTracking();

        // Filter RoleName
        if (!string.IsNullOrWhiteSpace(spec?.RoleName))
        {
            var roleNameTerm = spec.RoleName.Trim();
            query = query.Where(r => r.RoleName.Contains(roleNameTerm));
        }

        // Sorting
        if (spec?.SortDirection.HasValue == true)
        {
            query = spec.SortDirection == SortDirection.Desc
                ? query.OrderByDescending(r => r.RoleName)
                : query.OrderBy(r => r.RoleName);
        }
        else
        {
            query = query.OrderBy(r => r.RoleName);
        }

        var totalRecords = await query.CountAsync();
        var skip = (pageIndex - 1) * pageSize;
        var roles = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(r => new RoleResponse(r.RoleID, r.RoleName, r.Description))
            .ToListAsync();

        return new PaginationResult<IEnumerable<RoleResponse>>(
            roles,
            totalRecords,
            pageIndex,
            pageSize);
    }
}

