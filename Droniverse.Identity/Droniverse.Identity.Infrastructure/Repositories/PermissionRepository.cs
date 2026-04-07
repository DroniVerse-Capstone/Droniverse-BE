using Droniverse.Identity.Domain.Entities;
using Droniverse.Identity.Domain.Interfaces;
using Droniverse.Identity.Infrastructure.Persistence;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Identity.Infrastructure.Repositories;

internal class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(IdentityDbContext context) : base(context)
    {
    }

    public async Task<PaginationResult<IEnumerable<PermissionResponse>>> GetAllPermissionsAsync(
        IPermissionSearchSpecification spec,
        int pageIndex,
        int pageSize)
    {
        IQueryable<Permission> query = _dbSet.AsNoTracking();

        // Filter PermissionName
        if (!string.IsNullOrWhiteSpace(spec?.PermissionName))
        {
            var permissionNameTerm = spec.PermissionName.Trim();
            query = query.Where(p => p.PermissionName.Contains(permissionNameTerm));
        }

        // Sorting
        if (spec?.SortDirection.HasValue == true)
        {
            query = spec.SortDirection == SortDirection.Desc
                ? query.OrderByDescending(p => p.PermissionName)
                : query.OrderBy(p => p.PermissionName);
        }
        else
        {
            query = query.OrderBy(p => p.PermissionName);
        }

        var totalRecords = await query.CountAsync();
        var skip = (pageIndex - 1) * pageSize;
        var permissions = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(p => new PermissionResponse(p.PermissionID, p.PermissionName, p.Description))
            .ToListAsync();

        return new PaginationResult<IEnumerable<PermissionResponse>>(
            permissions,
            totalRecords,
            pageIndex,
            pageSize);
    }
}