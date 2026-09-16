using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository;

public interface ILevelCourseRequirementRepository : IRepository<LevelCourseRequirement>
{
    Task<List<LevelCourseRequirement>> GetByLevelIdAsync(Guid levelId, CancellationToken cancellationToken = default);

    void RemoveRange(IEnumerable<LevelCourseRequirement> entities);
}