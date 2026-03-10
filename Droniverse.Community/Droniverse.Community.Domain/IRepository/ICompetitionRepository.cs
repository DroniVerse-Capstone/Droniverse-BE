using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Domain.IRepository;
public interface ICompetitionRepository : IRepository<Competition>
{
    Task<Dictionary<Guid, int>> GetCompetitorCountsByCompetitionIds(IEnumerable<Guid> competitionIds);
    Task<Dictionary<Guid, int>> GetPrizeCountsByCompetitionIds(IEnumerable<Guid> competitionIds);
}

