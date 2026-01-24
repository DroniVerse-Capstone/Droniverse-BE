using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class RoundRepository : MySqlRepository<Round>, IRoundRepository
{
    public RoundRepository(MySqlDbContext context) : base(context)
    {
    }
}

