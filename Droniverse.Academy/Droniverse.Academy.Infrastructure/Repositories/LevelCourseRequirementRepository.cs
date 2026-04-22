using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class LevelCourseRequirementRepository : MySqlRepository<LevelCourseRequirement>, ILevelCourseRequirementRepository
{
    public LevelCourseRequirementRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<List<LevelCourseRequirement>> GetByLevelIdAsync(Guid levelId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => x.LevelID == levelId)
            .ToListAsync(cancellationToken);
    }

    public void RemoveRange(IEnumerable<LevelCourseRequirement> entities)
    {
        _dbSet.RemoveRange(entities);
    }
}