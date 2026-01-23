using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ParticipationRepository : Repository<Participation>, IParticipationRepository
{
    public ParticipationRepository(MySqlDbContext context) : base(context)
    {
    }
}

