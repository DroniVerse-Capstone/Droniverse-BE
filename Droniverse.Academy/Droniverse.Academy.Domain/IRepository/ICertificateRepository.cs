
using Droniverse.Academy.Domain.Entities;

namespace Droniverse.Academy.Domain.IRepository;
public interface ICertificateRepository : IRepository<Certificate>
{
    public Task GenerateCertificate(Guid userId, Guid courseId);
}

