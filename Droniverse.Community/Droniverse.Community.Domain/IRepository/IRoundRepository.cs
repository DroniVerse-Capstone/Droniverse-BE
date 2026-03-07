using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;
public interface IRoundRepository : IRepository<Round>
{

    Task<Dictionary<Guid, int>> GetRoundCountsByCompetitionIds(IEnumerable<Guid> competitionIds);
}

