using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Community.Infrastructure.QueryModels;
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

        if (ids.Count == 0)
            return [];

        return await _context.Set<Round>()
            .AsNoTracking()
            .Where(cc => ids.Contains(cc.CompetitionID))
            .GroupBy(cc => cc.CompetitionID)
            .Select(g => new { RoundID = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.RoundID, x => x.Count);
    }

    public async Task<RoundQueryModel?> GetRoundByRoundID(Guid roundID)
    {
        return await _context.Rounds
            .AsNoTracking()
            .Where(r => r.RoundID == roundID)
            .Select(r => new RoundQueryModel
            {
                RoundID = r.RoundID,
                CompetitionID = r.CompetitionID,
                NameVN = r.Competition.NameVN,
                NameEN = r.Competition.NameEN,
                VRSimulatorID = r.VRSimilatorID,
                RoundNumber = r.RoundNumber,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                TimeLimit = r.TimeLimit,
                Status = r.Status,
                TotalParticipants = r.UserRounds.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<RoundQueryModel>> GetRoundsByCompetitionID(
      Guid competitionID,
      RoundStatus? roundStatus = null)
    {
        var query = _context.Rounds
            .AsNoTracking()
            .Where(r => r.CompetitionID == competitionID);

        if (roundStatus.HasValue)
            query = query.Where(r => r.Status == roundStatus.Value);
        else
            query = query.Where(r => r.Status != RoundStatus.Cancelled);

        return await query
            .OrderBy(r => r.RoundNumber)
            .Select(r => new RoundQueryModel
            {
                RoundID = r.RoundID,
                CompetitionID = r.CompetitionID,
                NameVN = r.Competition.NameVN,
                NameEN = r.Competition.NameEN,
                VRSimulatorID = r.VRSimilatorID,
                RoundNumber = r.RoundNumber,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                TimeLimit = r.TimeLimit,
                Status = r.Status,
                TotalParticipants = r.UserRounds.Count()
            })
            .ToListAsync();
    }
    public async Task<RoundQueryModel?> GetCurrentRoundByCompetitionID(Guid competitionID)
    {
        var now = DateTime.UtcNow;

        return await _context.Rounds
            .AsNoTracking()
            .Where(r => r.CompetitionID == competitionID
                        && r.Status == RoundStatus.Valid
                        && r.StartTime <= now
                        && r.EndTime >= now)
            .OrderBy(r => r.RoundNumber)
            .Select(r => new RoundQueryModel
            {
                RoundID = r.RoundID,
                CompetitionID = r.CompetitionID,
                NameVN = r.Competition.NameVN,
                NameEN = r.Competition.NameEN,
                VRSimulatorID = r.VRSimilatorID,
                RoundNumber = r.RoundNumber,
                StartTime = r.StartTime,
                EndTime = r.EndTime,
                Status = r.Status,
                TotalParticipants = r.UserRounds.Count()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Round?> GetRoundForJoinById(Guid roundID)
    {
        return await _context.Rounds
            .AsNoTracking()
            .Include(r => r.Competition)
            .FirstOrDefaultAsync(r => r.RoundID == roundID);
    }

    public async Task<Round?> GetPreviousRoundByCompetition(Guid competitionID, int currentRoundNumber)
    {
        if (currentRoundNumber <= 1)
            return null;

        var previousRoundNumber = currentRoundNumber - 1;

        return await _context.Rounds
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.CompetitionID == competitionID && r.RoundNumber == previousRoundNumber);
    }
}

