using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseRepository : MySqlRepository<Course>, ICourseRepository
{
    public CourseRepository(MySqlDbContext context) : base(context)
    {
        
    }

    public async Task<Course?> GetByIdWithActiveVersionAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseID == id)
            .Include(c => c.CourseVersions
                .Where(v => v.Status == CourseVersionStatus.ACTIVE))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdWithAllVersionsAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseID == id)
            .Include(c => c.CourseVersions)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginationResult<IEnumerable<Course>>>
    GetAllWithActiveVersionAsync(
        Expression<Func<Course, bool>>? filter = null,
        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Course> query = _dbSet
            .Include(c => c.CourseVersions
                .Where(v => v.Status == CourseVersionStatus.ACTIVE));

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<IEnumerable<Course>>(
            items,
            totalCount,
            pageIndex,
            pageSize
        );
    }

    public async Task<PaginationResult<IEnumerable<Course>>>
    GetAllWithAllVersionsAsync(
        Expression<Func<Course, bool>>? filter = null,
        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Course> query = _dbSet
            .Include(c => c.CourseVersions);

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<IEnumerable<Course>>(
            items,
            totalCount,
            pageIndex,
            pageSize
        );
    }
}

