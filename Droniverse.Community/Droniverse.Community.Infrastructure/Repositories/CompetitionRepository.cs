using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class CompetitionRepository : Repository<Competition>, ICompetitionRepository
{
    public CompetitionRepository(MySqlDbContext context) : base(context)
    {
    }
}

