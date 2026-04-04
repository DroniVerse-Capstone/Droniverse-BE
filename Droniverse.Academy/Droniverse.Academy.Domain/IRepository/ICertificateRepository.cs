
using Droniverse.Academy.Domain.Entities;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICertificateRepository : IRepository<Certificate>
{
    Task<IEnumerable<SimpleCertificateResponse>> GetSimpleCertificatesByIdsAsync(IEnumerable<Guid> certificateIds, CancellationToken cancellationToken = default);
}

