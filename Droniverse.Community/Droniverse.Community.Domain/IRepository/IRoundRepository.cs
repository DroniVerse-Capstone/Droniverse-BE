using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Infrastructure.QueryModels;

namespace Droniverse.Community.Domain.IRepository;
public interface IRoundRepository : IRepository<Round>
{
    Task<Dictionary<Guid, int>> GetRoundCountsByCompetitionIds(IEnumerable<Guid> competitionIds);

    Task<RoundQueryModel?> GetRoundByRoundID(Guid roundID);

    Task<IEnumerable<RoundQueryModel>> GetRoundsByCompetitionID(Guid competitionID);

    Task<RoundQueryModel?> GetCurrentRoundByCompetitionID(Guid competitionID);

    Task<Round?> GetRoundForJoinById(Guid roundID);

    Task<Round?> GetPreviousRoundByCompetition(Guid competitionID, int currentRoundNumber);
}

