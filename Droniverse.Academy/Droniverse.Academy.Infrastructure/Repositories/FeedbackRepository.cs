using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class FeedbackRepository : MySqlRepository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<Dictionary<Guid, decimal>> GetAverageRatingsByCourseVersionIdsAsync(
        IEnumerable<Guid> courseVersionIds,
        CancellationToken cancellationToken = default)
    {
        var ids = courseVersionIds?.Distinct().ToList() ?? [];
        if (ids.Count == 0)
            return [];

        return await _dbSet
            .AsNoTracking()
            .Where(f => ids.Contains(f.CourseVersionID))
            .GroupBy(f => f.CourseVersionID)
            .Select(g => new
            {
                CourseVersionId = g.Key,
                AverageRating = g.Average(x => (decimal)x.Rating)
            })
            .ToDictionaryAsync(
                x => x.CourseVersionId,
                x => x.AverageRating,
                cancellationToken);
    }
}

