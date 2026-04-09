using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.IRepository;
public interface ICompetitionRepository : IRepository<Competition>
{
    Task<Dictionary<Guid, int>> GetCompetitorCountsByCompetitionIds(IEnumerable<Guid> competitionIds);
    Task<Dictionary<Guid, int>> GetPrizeCountsByCompetitionIds(IEnumerable<Guid> competitionIds);

    Task<Dictionary<Guid, (int RoundCount, int CompetitorCount, int PrizeCount)>> GetAggregateCountsByCompetitionIds(
        IEnumerable<Guid> competitionIds);

    Task<(IEnumerable<Competition> Items, int TotalCount)> GetFilteredCompetitionsAsync(
        string? competitionName,
        CompetitionStatus? status,
        DateTime? registrationStartDate,
        DateTime? registrationEndDate,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take);

}

