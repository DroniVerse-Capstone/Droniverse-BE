using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class RoundRepository : MySqlRepository<Round>, IRoundRepository
{
    public RoundRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<Dictionary<Guid, int>> GetRoundCountsByCompetitionIds(IEnumerable<Guid> competitionIds)
    {
        var ids = competitionIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<Round>()
            .AsNoTracking()
            .Where(cc => ids.Contains(cc.CompetitionID))
            .GroupBy(cc => cc.CompetitionID)
            .Select(g => new { RoundID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoundID, x => x.Count);
    }
}

