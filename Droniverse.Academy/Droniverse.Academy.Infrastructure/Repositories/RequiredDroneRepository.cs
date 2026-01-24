using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class RequiredDroneRepository : MySqlRepository<RequiredDrone>, IRequiredDroneRepository
{
    public RequiredDroneRepository(MySqlDbContext context) : base(context)
    {
    }
}

