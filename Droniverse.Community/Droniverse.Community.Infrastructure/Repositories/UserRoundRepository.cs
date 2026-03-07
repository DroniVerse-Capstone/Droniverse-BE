using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;

internal class UserRoundRepository : MySqlRepository<UserRound>, IUserRoundRepository
{
    public UserRoundRepository(MySqlDbContext context) : base(context)
    {
    }
}
