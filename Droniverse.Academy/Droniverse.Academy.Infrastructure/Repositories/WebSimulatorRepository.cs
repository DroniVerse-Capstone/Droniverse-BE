using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class WebSimulatorRepository : MySqlRepository<WebSimulator>, IWebSimulatorRepository
{
    public WebSimulatorRepository(MySqlDbContext context) : base(context)
    {
    }
}
