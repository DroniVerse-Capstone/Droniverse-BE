using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class CertificateRepository : MySqlRepository<Certificate>, ICertificateRepository
{
    public CertificateRepository(MySqlDbContext context) : base(context)
    {
    }
}

