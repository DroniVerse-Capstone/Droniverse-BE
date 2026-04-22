using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserLevelRepository : MySqlRepository<UserLevel>, IUserLevelRepository
{
    public UserLevelRepository(MySqlDbContext context) : base(context)
    {
    }
}
