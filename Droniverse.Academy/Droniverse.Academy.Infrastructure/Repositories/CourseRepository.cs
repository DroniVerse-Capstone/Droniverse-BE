using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.QueryModels;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CourseRepository : MySqlRepository<Course>, ICourseRepository
{
    public CourseRepository(MySqlDbContext context) : base(context)
    {

    }

    public async Task<Course?> GetByIdWithCurrentVersionAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseID == id)
            .Include(c => c.CurrentVersion)
            .Include(c => c.Level)
            .Include(c => c.Drone)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Course?> GetByIdWithAllVersionsAsync(
    Guid id,
    CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CourseID == id)
            .Include(c => c.CurrentVersion)
            .Include(c => c.CourseVersions)
            .Include(c => c.Level)
            .Include(c => c.Drone)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaginationResult<IEnumerable<Course>>>
    GetAllWithCurrentVersionAsync(
        Expression<Func<Course, bool>>? filter = null,
        Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
        int pageIndex = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Course> query = _dbSet
            .Include(c => c.CurrentVersion).Include(l => l.Level).Include(d => d.Drone);

        if (filter != null)
            query = query.Where(filter);

        query = orderBy != null
            ? orderBy(query)
            : query.OrderBy(c => c.CourseID);

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
            .Include(c => c.CurrentVersion)
            .Include(l => l.Level)
            .Include(d => d.Drone)
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

    public async Task<IEnumerable<Course>> GetAllWithCurrentVersionNoPagingAsync(
    Expression<Func<Course, bool>>? filter = null,
    Func<IQueryable<Course>, IOrderedQueryable<Course>>? orderBy = null,
    CancellationToken cancellationToken = default)
    {
        IQueryable<Course> query = _dbSet
            .Include(c => c.CurrentVersion)
            .Include(l => l.Level)
            .Include(d => d.Drone);

        if (filter != null)
            query = query.Where(filter);

        if (orderBy != null)
            query = orderBy(query);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<PaginationResult<IEnumerable<CourseBulkResponseDTO>>> GetHotCoursesByIdsWithCurrentVersionAsync(
        IEnumerable<Guid> courseIds,
        Guid currentUserId,
        bool ownedOnly,
        string? courseName,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
        {
            return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>([], 0, pageIndex, pageSize);
        }

        var keyword = courseName?.Trim();
        var normalizedPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var normalizedPageSize = pageSize < 1 ? 5 : pageSize;

        var courseQuery = _dbSet
            .AsNoTracking()
            .Where(c =>
                ids.Contains(c.CourseID) &&
                c.CurrentVersion != null &&
                c.Status == CourseStatus.PUBLISH);


        if (!string.IsNullOrWhiteSpace(keyword))
        {
            courseQuery = courseQuery.Where(c =>
                c.CurrentVersion!.TitleEN.Contains(keyword) ||
                c.CurrentVersion.TitleVN.Contains(keyword));
        }

        //if (ownedOnly)
        //{
        //    statsQuery =
        //        from stats in statsQuery
        //        join course in _dbSet.AsNoTracking() on stats.CourseID equals course.CourseID
        //        where course.CreateBy == currentUserId
        //        select stats;
        //}

        var rankedQuery = courseQuery.Select(c => new
        {
            Course = new CourseBulkResponseDTO
            {
                CourseId = c.CourseID,
                CourseVersionId = c.CurrentVersionID ?? Guid.Empty,
                TitleVN = c.CurrentVersion!.TitleVN,
                TitleEN = c.CurrentVersion.TitleEN,
                Level = c.Level == null ? null : new CourseLevelMiniResponseDTO
                {
                    LevelID = c.Level.LevelID,
                    LevelNumber = c.Level.LevelNumber,
                    Name = c.Level.Name
                },
                Drone = c.Drone == null ? null : new CourseDroneMiniResponseDTO
                {
                    DroneID = c.Drone.DroneID,
                    Name = c.Drone.DroneNameEN,
                    ImgURL = c.Drone.ImgURL
                },
                EstimatedDuration = c.CurrentVersion.EstimatedDuration,
                Price = null,
                Rating = c.CurrentVersion.Feedbacks
                    .Select(f => (decimal?)f.Rating)
                    .Average() ?? 0m,
                NumberOfParticipants = c.CurrentVersion.Enrollments.Count(e => e.Status == EnrollStatus.ACTIVE || e.Status == EnrollStatus.COMPLETED),
                ImageUrl = c.CurrentVersion.ImageUrl
            },
            ParticipantCount = c.CurrentVersion!.Enrollments.Count(e => e.Status == EnrollStatus.ACTIVE || e.Status == EnrollStatus.COMPLETED),
            AverageRating = c.CurrentVersion.Feedbacks
                .Select(f => (decimal?)f.Rating)
                .Average(),
            UpdateAt = c.CurrentVersion.UpdateAt,
            HotScore =
                (c.CurrentVersion.Enrollments.Count(e => e.Status == EnrollStatus.ACTIVE || e.Status == EnrollStatus.COMPLETED) * 0.6m) +
                (((c.CurrentVersion.Feedbacks.Select(f => (decimal?)f.Rating).Average()) ?? 0m) * 10m * 0.35m) +
                (c.CurrentVersion.UpdateAt.HasValue ? 1.5m : 0m)
        });

        var totalCount = await rankedQuery.CountAsync(cancellationToken);

        var items = await rankedQuery
            .OrderByDescending(x => x.HotScore)
            .ThenByDescending(x => x.ParticipantCount)
            .ThenByDescending(x => x.AverageRating)
            .ThenByDescending(x => x.UpdateAt)
            .ThenBy(x => x.Course.CourseId)
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => x.Course)
            .ToListAsync(cancellationToken);

        return new PaginationResult<IEnumerable<CourseBulkResponseDTO>>(items, totalCount, normalizedPageIndex, normalizedPageSize);
    }

    public async Task<IEnumerable<SimpleCourseResponse>> GetSimpleCoursesByIdsAsync(
        IEnumerable<Guid> courseIds,
        CancellationToken cancellationToken = default)
    {
        var ids = courseIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? [];

        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbSet
            .AsNoTracking()
            .Where(c => ids.Contains(c.CourseID) && c.CurrentVersion != null && c.Status == CourseStatus.PUBLISH)
            .Select(c => new SimpleCourseResponse
            {
                CourseId = c.CourseID,
                CourseNameVN = c.CurrentVersion!.TitleVN,
                CourseNameEN = c.CurrentVersion.TitleEN,
                ImageUrl = c.CurrentVersion.ImageUrl ?? string.Empty
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CourseInfoQueryModel?> GetCourseInfoByIdAsync(Guid courseId)
    {
        return await _dbSet
            .Where(c => c.CourseID == courseId)
            .Select(c => new CourseInfoQueryModel
            {
                CourseId = c.CourseID,
                CourseNameVN = c.CurrentVersion!.TitleVN,
                CourseNameEN = c.CurrentVersion.TitleEN,
                CourseStatus = c.Status
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CourseInfoQueryModel>?> GetCourseInfoByIdAsync(List<Guid> courseIds)
    {
        if (courseIds == null || courseIds.Count == 0)
            return [];

        var distinctIds = courseIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinctIds.Count == 0)
            return [];

        return await _dbSet
            .Where(c => distinctIds.Contains(c.CourseID))
            .Select(c => new CourseInfoQueryModel
            {
                CourseId = c.CourseID,
                CourseNameVN = c.CurrentVersion!.TitleVN,
                CourseNameEN = c.CurrentVersion!.TitleEN,
                CourseStatus = c.Status
            })
            .AsNoTracking()
            .ToListAsync();
    }

}

