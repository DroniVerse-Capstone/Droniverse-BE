using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubRepository : MySqlRepository<Club>, IClubRepository
{
    public ClubRepository(MySqlDbContext context) : base(context)
    {
    }
}

