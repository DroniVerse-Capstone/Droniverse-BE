using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository;

public interface IFeedbackRepository : IRepository<Feedback>
{
    Task<Dictionary<Guid, decimal>> GetAverageRatingsByCourseVersionIdsAsync(
        IEnumerable<Guid> courseVersionIds,
        CancellationToken cancellationToken = default);
}

