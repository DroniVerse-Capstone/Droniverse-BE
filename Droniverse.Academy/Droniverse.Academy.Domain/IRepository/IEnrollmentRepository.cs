using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository;

public interface IEnrollmentRepository : IRepository<Enrollment>
{
    Task<Dictionary<Guid, int>> GetActiveOrCompletedParticipantCountsByCourseVersionIdsAsync(
        IEnumerable<Guid> courseVersionIds,
        CancellationToken cancellationToken = default);
}

