using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class CompetitionRepository : MySqlRepository<Competition>, ICompetitionRepository
{
    public CompetitionRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<Dictionary<Guid, int>> GetCompetitorCountsByCompetitionIds(IEnumerable<Guid> competitionIds)
    {
        var ids = competitionIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<UserCompetition>()
            .Where(uc => ids.Contains(uc.CompetitionID))
            .GroupBy(uc => uc.CompetitionID)
            .Select(g => new { CompetitionID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompetitionID, x => x.Count);
    }

    public async Task<Dictionary<Guid, int>> GetPrizeCountsByCompetitionIds(IEnumerable<Guid> competitionIds)
    {
        var ids = competitionIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<CompetitionPrize>()
            .Where(cp => ids.Contains(cp.CompetitionID))
            .GroupBy(cp => cp.CompetitionID)
            .Select(g => new { CompetitionID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompetitionID, x => x.Count);
    }
}

