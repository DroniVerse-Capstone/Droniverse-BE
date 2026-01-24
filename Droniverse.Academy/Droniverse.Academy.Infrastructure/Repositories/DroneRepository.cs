using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class DroneRepository : MySqlRepository<Drone>, IDroneRepository
{
    public DroneRepository(MySqlDbContext context) : base(context)
    {
    }
}

