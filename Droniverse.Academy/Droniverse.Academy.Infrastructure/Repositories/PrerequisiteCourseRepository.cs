using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class PrerequisiteCourseRepository : MySqlRepository<PrerequisiteCourse>, IPrerequisiteCourseRepository
{
    public PrerequisiteCourseRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<List<PrerequisiteCourse>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.CourseID == courseId)
            .ToListAsync(cancellationToken);
    }

    public void RemoveRange(IEnumerable<PrerequisiteCourse> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}
