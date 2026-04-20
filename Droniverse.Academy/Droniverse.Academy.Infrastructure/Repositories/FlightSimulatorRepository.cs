using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class FlightSimulatorRepository : MySqlRepository<FlightSimulator>, IFlightSimulatorRepository
{
    public FlightSimulatorRepository(MySqlDbContext context) : base(context)
    {
    }
}
