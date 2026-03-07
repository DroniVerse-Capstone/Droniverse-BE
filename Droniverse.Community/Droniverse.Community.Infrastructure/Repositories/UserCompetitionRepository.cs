using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Infrastructure.Repositories
{
    public class UserCompetitionRepository : MySqlRepository<UserCompetition>, IUserCompetitionRepository
    {
        public UserCompetitionRepository(MySqlDbContext context) : base(context)
        {
        }

        public async Task<Dictionary<Guid, int>> GetCompetitorCountsByCompetitionIds(IEnumerable<Guid> competitionIds)
        {
            var ids = competitionIds.Distinct().ToList();
            if (!ids.Any())
                return new Dictionary<Guid, int>();

            return await _context.Set<UserCompetition>()
                .Where(cc => ids.Contains(cc.CompetitionID))
                .GroupBy(cc => cc.CompetitionID)
                .Select(g => new { UserCompetitionID = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserCompetitionID, x => x.Count);
        }
    }
}
