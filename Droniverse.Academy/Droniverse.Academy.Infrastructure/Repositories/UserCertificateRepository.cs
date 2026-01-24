using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserCertificateRepository : MySqlRepository<UserCertificate>, IUserCertificateRepository
{
    public UserCertificateRepository(MySqlDbContext context) : base(context)
    {
    }
}

