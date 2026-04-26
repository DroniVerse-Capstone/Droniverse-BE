using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Community.Infrastructure.QueryModels;
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

        public async Task<(int TotalRecords, IEnumerable<CompetitionLeaderboardQueryModel> Entries)> GetCompetitionLeaderboard(
            Guid competitionId,
            int skip,
            int take)
        {
            var query = _context.UserCompetitions
                .AsNoTracking()
                .Where(uc => uc.CompetitionID == competitionId && uc.Status == UserCompetitionStatus.ACTIVE)
                .OrderByDescending(uc => uc.Score)
                .ThenBy(uc => uc.UpdatedAt);

            var totalRecords = await query.CountAsync();
            if (totalRecords == 0)
                return (0, []);

            var entries = await query
                .Skip(skip)
                .Take(take)
                .Select(uc => new CompetitionLeaderboardQueryModel
                {
                    UserId = uc.UserID,
                    Score = uc.Score ?? 0,
                    Rank = uc.Rank,
                    Status = uc.Status
                })
                .ToListAsync();

            return (totalRecords, entries);
        }

        public async Task<(int TotalRecords, IEnumerable<CompetitionParticipantQueryModel> Participants)> GetCompetitionParticipants(
            Guid competitionId,
            UserCompetitionStatus status,
            DateTime? joinFrom,
            int skip,
            int take)
        {
            var query = ApplyCompetitionParticipantFilters(competitionId, status, joinFrom);

            var totalRecords = await query.CountAsync();

            if (totalRecords == 0)
                return (0, []);

            var participants = await query
                .OrderByDescending(uc => uc.CreatedAt)
                .Skip(skip)
                .Take(take)
                .Select(uc => new CompetitionParticipantQueryModel
                {
                    UserId = uc.UserID,
                    Status = uc.Status,
                    Score = uc.Score,
                    Rank = uc.Rank,
                    PrizeId = uc.PrizeID,
                    CreatedAt = uc.CreatedAt,
                    UpdatedAt = uc.UpdatedAt
                })
                .ToListAsync();

            return (totalRecords, participants);
        }

        private IQueryable<UserCompetition> ApplyCompetitionParticipantFilters(
            Guid competitionId,
            UserCompetitionStatus status,
            DateTime? joinFrom)
        {
            var query = _context.UserCompetitions
                .AsNoTracking()
                .Where(uc => uc.CompetitionID == competitionId && uc.Status == status);

            if (joinFrom.HasValue)
            {
                var from = joinFrom.Value.Date;
                var to = from.AddDays(1);
                query = query.Where(uc => uc.CreatedAt >= from && uc.CreatedAt < to);
            }

            return query;
        }

        public async Task<bool> IsUserInCompetitionAsync(Guid competitionId, Guid userId)
        {
            return await _context.UserCompetitions.AsNoTracking().AnyAsync(uc => uc.UserID == userId && uc.CompetitionID == competitionId);
        }
    }
}
