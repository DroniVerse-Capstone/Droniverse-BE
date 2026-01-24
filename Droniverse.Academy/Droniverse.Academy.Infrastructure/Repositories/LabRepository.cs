using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class LabRepository : MySqlRepository<Lab>, ILabRepository
{
    public LabRepository(MySqlDbContext context) : base(context)
    {
    }
}

