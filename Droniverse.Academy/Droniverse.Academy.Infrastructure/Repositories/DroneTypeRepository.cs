using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class DroneTypeRepository : MySqlRepository<DroneType>, IDroneTypeRepository
{
    public DroneTypeRepository(MySqlDbContext context) : base(context)
    {
    }
}

