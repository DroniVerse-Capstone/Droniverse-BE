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
    public class CompetitionPrizeRepository : MySqlRepository<CompetitionPrize>, ICompetitionPrizeRepository
    {
        public CompetitionPrizeRepository(MySqlDbContext context) : base(context)
        {
        }

        public async Task<bool> HasCompetitionPrizes(Guid competitionId)
        {
            return await _context.CompetitionPrizes
                .AsNoTracking()
                .AnyAsync(x => x.CompetitionID == competitionId);
        }
    }
}
