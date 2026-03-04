using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubAttemptRequestRepository : MySqlRepository<ClubAttemptRequest>, IClubAttemptRequestRepository
{
    public ClubAttemptRequestRepository(MySqlDbContext context) : base(context)
    {
    }
}

