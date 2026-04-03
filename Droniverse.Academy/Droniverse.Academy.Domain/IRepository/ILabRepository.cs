using Droniverse.Academy.Domain.Entities;
using Droniverse.Community.Application.DTO.Response;

namespace Droniverse.Academy.Domain.IRepository;
public interface ILabRepository : IRepository<Lab>
{
    Task<bool> IsExistAsync(Guid labId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SimpleLabResponse>> GetSimpleLabsByIdsAsync(IEnumerable<Guid> labIds, CancellationToken cancellationToken = default);
}

