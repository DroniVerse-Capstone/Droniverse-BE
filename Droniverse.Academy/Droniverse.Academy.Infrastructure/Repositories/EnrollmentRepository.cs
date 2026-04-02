using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
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
}

