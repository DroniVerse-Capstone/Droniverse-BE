using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubRepository : MySqlRepository<Club>, IClubRepository
{
    public ClubRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Club>> GetAllWithCategories()
    {
        return await _context.Set<Club>()
            .AsNoTracking()
            .Include(c => c.ClubCategories)
            .ThenInclude(cc => cc.Category)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Club?> GetByIdWithCategories(Guid clubId)
    {
        return await _context.Set<Club>()
            .Include(c => c.ClubCategories)
            .ThenInclude(cc => cc.Category)
            .FirstOrDefaultAsync(c => c.ClubID == clubId);
    }

    public async Task<Club?> GetByClubCodeWithCategories(string clubCode)
    {
        return await _context.Set<Club>()
            .AsNoTracking()
            .Include(c => c.ClubCategories)
            .ThenInclude(cc => cc.Category)
            .FirstOrDefaultAsync(c => c.ClubCode == clubCode);
    }

    public async Task<IEnumerable<Club>> GetClubsByParticipantUserId(Guid userId, ClubStatus? status = null)
    {
        var query = _context.Set<Participation>()
            .AsNoTracking()
            .Where(p => p.UserID == userId &&
                        p.Status == ParticipationStatus.ACTIVE &&
                        (!status.HasValue || p.Club.Status == status))
            .Include(p => p.Club)
                .ThenInclude(c => c.ClubCategories)
                    .ThenInclude(cc => cc.Category)
            .Where(p => p.Club != null)
            .Select(p => p.Club)
            .Distinct();

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Club>> GetClubsByClubManagerID(Guid clubManagerID, ClubStatus? status = null)
    {
        var query = _context.Set<Club>()
            .AsNoTracking()
            .Where(c => c.CreatedBy == clubManagerID && (!status.HasValue || c.Status == status))
            .Include(c => c.ClubCategories)
                .ThenInclude(cc => cc.Category)
            .OrderByDescending(c => c.CreatedAt);

        return await query.ToListAsync();
    }

    public async Task<Dictionary<Guid, int>> GetMemberCountsByClubIds(IEnumerable<Guid> clubIds)
    {
        var ids = clubIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<Participation>()
            .Where(p => ids.Contains(p.ClubID) && p.Status == ParticipationStatus.ACTIVE)
            .GroupBy(p => p.ClubID)
            .Select(g => new { ClubID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubID, x => x.Count);
    }

    public async Task<Dictionary<Guid, int>> GetCourseCountsByClubIds(IEnumerable<Guid> clubIds)
    {
        var ids = clubIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<ClubCourse>()
            .Where(cc => ids.Contains(cc.ClubID))
            .GroupBy(cc => cc.ClubID)
            .Select(g => new { ClubID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubID, x => x.Count);
    }
}

