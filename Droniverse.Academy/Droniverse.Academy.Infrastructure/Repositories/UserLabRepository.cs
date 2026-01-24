using Droniverse.Academy.Domain.Entities;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Academy.Infrastructure.Persistence.MySql;

namespace Droniverse.Academy.Infrastructure.Repositories;

internal class UserLabRepository : MySqlRepository<UserLab>, IUserLabRepository
{
    public UserLabRepository(MySqlDbContext context) : base(context)
    {
    }
}

