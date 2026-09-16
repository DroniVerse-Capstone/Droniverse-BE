using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.Models;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICourseVersionRepository : IRepository<CourseVersion>
{
    Task<CourseOverviewData?> GetCourseOverviewDataAsync(
        Guid courseVersionId,
        Guid userId,
        CancellationToken cancellationToken = default);
}

