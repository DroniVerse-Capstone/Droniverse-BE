using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository;

public interface IPrerequisiteCourseRepository : IRepository<PrerequisiteCourse>
{
    Task<List<PrerequisiteCourse>> GetByCourseIdAsync(Guid courseId, CancellationToken cancellationToken = default);
    Task<List<PrerequisiteCourse>> GetByCourseIdsWithRequiredCourseAsync(
        IEnumerable<Guid> courseIds,
        CancellationToken cancellationToken = default);
    void RemoveRange(IEnumerable<PrerequisiteCourse> entities);
}
