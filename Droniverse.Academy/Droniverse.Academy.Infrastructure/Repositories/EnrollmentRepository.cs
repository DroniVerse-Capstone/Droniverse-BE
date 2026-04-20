using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Domain.QueryModels;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs.Response;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class EnrollmentRepository : MySqlRepository<Enrollment>, IEnrollmentRepository
{
    public EnrollmentRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<Dictionary<Guid, int>> GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(
        IEnumerable<Guid> courseVersionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = courseVersionIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(e =>
                ids.Contains(e.CourseVersionID) &&
                (e.Status == EnrollStatus.ACTIVE || e.Status == EnrollStatus.COMPLETED))
            .GroupBy(e => e.CourseVersionID)
            .Select(g => new
            {
                CourseVersionId = g.Key,
                ParticipantCount = g.Count()
            })
            .ToDictionaryAsync(
                x => x.CourseVersionId,
                x => x.ParticipantCount,
                cancellationToken);
    }

    public async Task<PaginationResult<IEnumerable<CourseEnrollmentQueryModel>>> GetCoursesOfUserAsync(
        Guid userId,
        Guid clubId,
        int pageIndex,
        int pageSize,
        string? courseSearchName = null,
        EnrollStatus? enrollmentStatus = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageIndex = pageIndex < 1 ? 1 : pageIndex;
        var normalizedPageSize = pageSize < 1 ? 5 : pageSize;
        var keyword = courseSearchName?.Trim();

        var query = _dbSet
            .AsNoTracking()
            .Where(e => e.UserID == userId && e.ClubID == clubId);

        if (enrollmentStatus.HasValue)
        {
            var status = enrollmentStatus.Value;
            query = query.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(e =>
                e.CourseVersion.TitleVN.Contains(keyword) ||
                e.CourseVersion.TitleEN.Contains(keyword));
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.LastAccessDate)
            .Select(e => new CourseEnrollmentQueryModel
            {
                EnrollmentId = e.EnrollmentID,
                CourseId = e.CourseID,
                CourseVersionId = e.CourseVersionID,
                CourseNameVN = e.CourseVersion.TitleVN,
                CourseNameEN = e.CourseVersion.TitleEN,
                ImageUrl = e.CourseVersion.ImageUrl,
                EstimatedDuration = e.CourseVersion.EstimatedDuration,
                Progress = e.Progress,
                EnrollStatus = e.Status
            })
            .Skip((normalizedPageIndex - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PaginationResult<IEnumerable<CourseEnrollmentQueryModel>>(
            items,
            totalRecords,
            normalizedPageIndex,
            normalizedPageSize);
    }
}

