using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
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
        var ids = competitionIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<UserCompetition>()
            .AsNoTracking()
            .Where(uc => ids.Contains(uc.CompetitionID))
            .GroupBy(uc => uc.CompetitionID)
            .Select(g => new { CompetitionID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompetitionID, x => x.Count);
    }

    public async Task<Dictionary<Guid, int>> GetPrizeCountsByCompetitionIds(IEnumerable<Guid> competitionIds)
    {
        var ids = competitionIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (!ids.Any())
            return new Dictionary<Guid, int>();

        return await _context.Set<CompetitionPrize>()
            .AsNoTracking()
            .Where(cp => ids.Contains(cp.CompetitionID))
            .GroupBy(cp => cp.CompetitionID)
            .Select(g => new { CompetitionID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.CompetitionID, x => x.Count);
    }

    public async Task<(IEnumerable<Competition> Items, int TotalCount)> GetFilteredCompetitionsAsync(
        string? competitionName,
        CompetitionStatus? status,
        DateTime? registrationStartDate,
        DateTime? registrationEndDate,
        DateTime? startDate,
        DateTime? endDate,
        int skip,
        int take)
    {
        var query = _context.Set<Competition>()
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(competitionName))
        {
            var keyword = competitionName.Trim();
            query = query.Where(c =>
                c.NameVN.Contains(keyword) ||
                c.NameEN.Contains(keyword));
        }

        if (registrationStartDate.HasValue)
        {
            query = query.Where(c => c.RegistrationStartDate >= registrationStartDate.Value);
        }

        if (registrationEndDate.HasValue)
        {
            var toDate = registrationEndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.RegistrationEndDate <= toDate);
        }

        if (startDate.HasValue)
        {
            query = query.Where(c => c.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            var toDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(c => c.EndDate <= toDate);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Dictionary<Guid, (int RoundCount, int CompetitorCount, int PrizeCount)>> GetAggregateCountsByCompetitionIds(
        IEnumerable<Guid> competitionIds)
    {
        var ids = competitionIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return [];

        var aggregates = await _context.Set<Competition>()
            .AsNoTracking()
            .Where(c => ids.Contains(c.CompetitionID))
            .Select(c => new
            {
                c.CompetitionID,
                RoundCount = _context.Set<Round>().Count(r => r.CompetitionID == c.CompetitionID),
                CompetitorCount = _context.Set<UserCompetition>().Count(uc => uc.CompetitionID == c.CompetitionID),
                PrizeCount = _context.Set<CompetitionPrize>().Count(cp => cp.CompetitionID == c.CompetitionID)
            })
            .ToListAsync();

        return aggregates.ToDictionary(
            x => x.CompetitionID,
            x => (x.RoundCount, x.CompetitorCount, x.PrizeCount));
    }
}

