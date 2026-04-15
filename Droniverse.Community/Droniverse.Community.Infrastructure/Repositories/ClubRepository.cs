using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Shared.DTOs;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;
internal class ClubRepository : MySqlRepository<Club>, IClubRepository
{
    public ClubRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<PaginationResult<IEnumerable<Club>>> GetAllWithCategories(
        string? clubName = null,
        ClubStatus? clubStatus = null,
        int currentPage = 1,
        int pageSize = 5)
    {
        currentPage = currentPage < 1 ? 1 : currentPage;
        pageSize = pageSize < 5 ? 5 : (pageSize > 20 ? 20 : pageSize);

        var query = _context.Set<Club>()
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(clubName))
        {
            var keyword = clubName.Trim();
            query = query.Where(c =>
                c.NameVN.Contains(keyword) ||
                c.NameEN.Contains(keyword));
        }

        if (clubStatus.HasValue)
            query = query.Where(c => c.Status == clubStatus.Value);

        var totalRecords = await query.CountAsync();

        query = query
                .Include(c => c.ClubCategories)
                .ThenInclude(cc => cc.Category);

        var data = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginationResult<IEnumerable<Club>>(data, totalRecords, currentPage, pageSize);
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

    public async Task<Dictionary<Guid, (int MemberCount, int CourseCount)>> GetClubStatsByClubIds(IEnumerable<Guid> clubIds)
    {
        var ids = clubIds.Distinct().ToList();
        if (!ids.Any())
            return new Dictionary<Guid, (int MemberCount, int CourseCount)>();

        var memberCounts = await _context.Set<Participation>()
            .Where(p => ids.Contains(p.ClubID) && p.Status == ParticipationStatus.ACTIVE)
            .GroupBy(p => p.ClubID)
            .Select(g => new { ClubID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubID, x => x.Count);

        var courseCounts = await _context.Set<ClubCourse>()
            .Where(cc => ids.Contains(cc.ClubID))
            .GroupBy(cc => cc.ClubID)
            .Select(g => new { ClubID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ClubID, x => x.Count);

        return ids.ToDictionary(
            id => id,
            id => (
                memberCounts.GetValueOrDefault(id, 0),
                courseCounts.GetValueOrDefault(id, 0)
            ));
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

    public async Task<SimpleClubResponse?> GetSimpleClubInfoById(Guid clubId)
    {
        return await _dbSet
            .Where(c => c.ClubID == clubId)
            .Select(c => new SimpleClubResponse
            {
                ClubId = c.ClubID,
                ClubNameVN = c.NameVN,
                ClubNameEN = c.NameEN,
                ImageUrl = c.ImageUrl!,
                ClubStatus = c.Status
            })
            .FirstOrDefaultAsync();
    }
}

