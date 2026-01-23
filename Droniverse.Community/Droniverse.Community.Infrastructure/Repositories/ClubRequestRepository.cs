using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubRequestRepository : Repository<ClubRequest>, IClubRequestRepository
{
    public ClubRequestRepository(MySqlDbContext context) : base(context)
    {
    }
}

