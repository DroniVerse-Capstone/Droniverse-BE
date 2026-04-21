using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class VRSimulatorRepository : MySqlRepository<VRSimulator>, IVRSimulatorRepository
{
    public VRSimulatorRepository(MySqlDbContext context) : base(context)
    {
    }
}
