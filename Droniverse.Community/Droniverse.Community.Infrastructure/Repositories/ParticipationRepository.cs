using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Shared.Enums;
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

    public async Task<List<Guid>> GetActiveParticipantUserIdsByClubAsync(Guid clubId)
    {
        return await _context.Set<Participation>()
            .AsNoTracking()
            .Where(p => p.ClubID == clubId && p.Status == ParticipationStatus.ACTIVE)
            .Select(p => p.UserID)
            .Distinct()
            .ToListAsync();
    }

    public async Task<(int TotalRecords, IEnumerable<Participation> Participations)> GetActiveParticipationsByClubAsync(
        Guid clubId,
        int skip,
        int take,
        IEnumerable<Guid>? userIdsFilter = null)
    {
        var query = _context.Set<Participation>()
            .AsNoTracking()
            .Where(p => p.ClubID == clubId && p.Status == ParticipationStatus.ACTIVE);

        if (userIdsFilter != null)
        {
            var filteredIds = userIdsFilter
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (filteredIds.Count == 0)
                return (0, []);

            query = query.Where(p => filteredIds.Contains(p.UserID));
        }

        var totalRecords = await query.CountAsync();
        if (totalRecords == 0)
            return (0, []);

        var participations = await query
            .OrderByDescending(p => p.JoinDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (totalRecords, participations);



    }

    public async Task<List<Guid>> GetParicipantIdsByClubId(Guid clubId, ParticipationStatus participationStatus)
    {
        return await _dbSet.Where(p => p.ClubID == clubId && p.Status == participationStatus).Select(p => p.UserID).ToListAsync();
    }
}

