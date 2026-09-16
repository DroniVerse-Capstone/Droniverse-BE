using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserSimulatorRepository : MySqlRepository<UserSimulator>, IUserSimulatorRepository
{
    public UserSimulatorRepository(MySqlDbContext context) : base(context)
    {
    }
}