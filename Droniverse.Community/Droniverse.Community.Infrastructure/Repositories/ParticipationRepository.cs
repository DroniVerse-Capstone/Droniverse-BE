using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ParticipationRepository : MySqlRepository<Participation>, IParticipationRepository
{
    public ParticipationRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<int> CountMembersByClubIdAsync(Guid clubId)
    {
        return await _context.Set<Participation>()
            .Where(p => p.ClubID == clubId
                     && p.Status == ParticipationStatus.ACTIVE)
            .CountAsync();
    }

    public async Task<bool> IsUserInClub(Guid clubId, Guid userId)
    {
        return await _context.Set<Participation>().AnyAsync(c => c.ClubID == clubId && c.UserID == userId && c.Status == ParticipationStatus.ACTIVE);
    }
}

