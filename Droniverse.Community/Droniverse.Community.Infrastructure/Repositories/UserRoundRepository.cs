using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using Droniverse.Community.Domain.IRepository;
using Droniverse.Community.Infrastructure.Persistence.MySql;
using Droniverse.Community.Infrastructure.QueryModels;
using Droniverse.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Droniverse.Community.Infrastructure.Repositories;

internal class UserRoundRepository : MySqlRepository<UserRound>, IUserRoundRepository
{
    public UserRoundRepository(MySqlDbContext context) : base(context)
    {
    }

    public async Task<bool> IsUserJoinedRound(Guid userId, Guid roundId)
    {
        return await _context.UserRounds
            .AsNoTracking()
            .AnyAsync(ur => ur.UserID == userId && ur.RoundID == roundId);
    }

    public async Task<bool> IsUserPassedRound(Guid userId, Guid roundId)
    {
        return await _context.UserRounds
            .AsNoTracking()
            .AnyAsync(ur => ur.UserID == userId
                            && ur.RoundID == roundId
                            && ur.IsPassed == true
                            && ur.Status == UserRoundStatus.Completed);
    }

    public async Task DisqualifyByCompetitionAsync(Guid competitionId, Guid userId, DateTime now)
    {
        // 1. Lấy danh sách RoundID thuộc competition
        var roundIds = await _context.Rounds
            .Where(r => r.CompetitionID == competitionId)
            .Select(r => r.RoundID)
            .ToListAsync();

        if (roundIds.Count == 0)
            return;

        // 2. Bulk update UserRound (KHÔNG load về memory)
        await _context.UserRounds
            .Where(ur => ur.UserID == userId
                      && roundIds.Contains(ur.RoundID)
                      && ur.Status != UserRoundStatus.Disqualified)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.Status, UserRoundStatus.Disqualified)
                .SetProperty(x => x.UpdatedAt, now)
                .SetProperty(x => x.SubmittedAt, now)
            );
    }

    public async Task<UserRoundDetailQueryModel?> GetRoundResultByUser(Guid userId, Guid roundId)
    {
        return await _context.UserRounds
            .AsNoTracking()
            .Where(ur => ur.UserID == userId && ur.RoundID == roundId)
            .Select(ur => new UserRoundDetailQueryModel
            {
                UserRoundId = ur.UserRoundID,
                Status = ur.Status,
                Point = ur.Point,
                ExecutionTime = ur.ExecutionTime,
                StartedAt = ur.StartedAt,
                SubmittedAt = ur.SubmittedAt,
                IsPassed = ur.IsPassed,
                Rank = ur.Rank,
                RoundId = ur.RoundID,
                RoundNumber = ur.Round.RoundNumber,
                RoundStartTime = ur.Round.StartTime,
                RoundEndTime = ur.Round.EndTime,
                RoundTimeLimit = ur.Round.TimeLimit,
                RoundStatus = ur.Round.Status
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> CountRoundResults(Guid roundId, UserRoundStatus? status)
    {
        var query = ApplyRoundResultFilters(roundId, status);
        return await query.CountAsync();
    }

    public async Task<IEnumerable<RoundResultParticipantQueryModel>> GetRoundResults(
        Guid roundId,
        UserRoundStatus? status,
        RoundResultAllSortBy? sortBy,
        SortDirection? sortDirection,
        int skip,
        int take)
    {
        var query = ApplyRoundResultFilters(roundId, status);
        query = ApplyRoundResultSorting(query, sortBy, sortDirection);

        return await query
            .Skip(skip)
            .Take(take)
            .Select(ur => new RoundResultParticipantQueryModel
            {
                UserId = ur.UserID,
                Status = ur.Status,
                StartedAt = ur.StartedAt,
                SubmittedAt = ur.SubmittedAt,
                IsPassed = ur.IsPassed,
                Rank = ur.Rank
            })
            .ToListAsync();
    }

    public async Task<int> CountMyRounds(
        Guid userId,
        UserRoundStatus? userRoundStatus,
        RoundStatus? roundStatus,
        bool? isPassed)
    {
        var query = ApplyMyRoundFilters(userId, userRoundStatus, roundStatus, isPassed);
        return await query.CountAsync();
    }

    public async Task<IEnumerable<MyRoundQueryModel>> GetMyRounds(
        Guid userId,
        UserRoundStatus? userRoundStatus,
        RoundStatus? roundStatus,
        bool? isPassed,
        int skip,
        int take)
    {
        var query = ApplyMyRoundFilters(userId, userRoundStatus, roundStatus, isPassed);

        return await query
            .OrderByDescending(ur => ur.StartedAt)
            .Skip(skip)
            .Take(take)
            .Select(ur => new MyRoundQueryModel
            {
                RoundId = ur.RoundID,
                VRSimulatorId = ur.Round.VRSimilatorID,
                RoundNumber = ur.Round.RoundNumber,
                StartTime = ur.Round.StartTime,
                EndTime = ur.Round.EndTime,
                TimeLimit = ur.Round.TimeLimit,
                RoundStatus = ur.Round.Status,
                Status = ur.Status,
                StartedAt = ur.StartedAt,
                SubmittedAt = ur.SubmittedAt,
                IsPassed = ur.IsPassed,
                Rank = ur.Rank
            })
            .ToListAsync();
    }

    public async Task<int> CountRoundParticipants(
        Guid roundId,
        DateTime? participantStartedFrom,
        DateTime? participantStartedEnd,
        DateTime? participationSubmittedFrom,
        DateTime? participationSubmittedEnd,
        UserRoundStatus? participantStatus,
        bool? isPassed,
        IReadOnlyCollection<Guid>? userIds)
    {
        var query = ApplyParticipantFilters(
            roundId,
            participantStartedFrom,
            participantStartedEnd,
            participationSubmittedFrom,
            participationSubmittedEnd,
            participantStatus,
            isPassed,
            userIds);

        return await query.CountAsync();
    }

    public async Task<IEnumerable<RoundParticipantQueryModel>> GetRoundParticipants(
        Guid roundId,
        DateTime? participantStartedFrom,
        DateTime? participantStartedEnd,
        DateTime? participationSubmittedFrom,
        DateTime? participationSubmittedEnd,
        UserRoundStatus? participantStatus,
        bool? isPassed,
        IReadOnlyCollection<Guid>? userIds,
        int skip,
        int take)
    {
        var query = ApplyParticipantFilters(
            roundId,
            participantStartedFrom,
            participantStartedEnd,
            participationSubmittedFrom,
            participationSubmittedEnd,
            participantStatus,
            isPassed,
            userIds);

        return await query
            .OrderByDescending(ur => ur.StartedAt)
            .Skip(skip)
            .Take(take)
            .Select(ur => new RoundParticipantQueryModel
            {
                UserId = ur.UserID,
                StartedAt = ur.StartedAt,
                Status = ur.Status,
                SubmittedAt = ur.SubmittedAt,
                IsPassed = ur.IsPassed
            })
            .ToListAsync();
    }

    public async Task<(int TotalRecords, IEnumerable<CompetitionLeaderboardQueryModel> Entries)> GetCompetitionLeaderboard(
        Guid competitionId,
        int skip,
        int take)
    {
        // 🔥 Step 1: Lấy data thô từ DB (KHÔNG group, KHÔNG sum)
        var raw = await _context.UserRounds
            .AsNoTracking()
            .Where(ur =>
                ur.Round.CompetitionID == competitionId &&
                ur.Status == UserRoundStatus.Completed)
            .Select(ur => new
            {
                ur.UserID,
                Score = (ur.Point ?? 0) * ur.Round.Weight,
                ExecutionTime = ur.ExecutionTime,
                LastSubmit = ur.SubmittedAt ?? ur.UpdatedAt
            })
            .ToListAsync(); // 🔥 cực kỳ quan trọng

        if (raw.Count == 0)
            return (0, []);

        // 🔥 Step 2: Group + tính toán ở memory
        var grouped = raw
            .GroupBy(x => x.UserID)
            .Select(g => new
            {
                UserId = g.Key,

                Score = g.Sum(x => x.Score),

                TotalTime = TimeSpan.FromMilliseconds(
                    g.Sum(x => (x.ExecutionTime ?? TimeSpan.Zero).TotalMilliseconds)
                ),

                LastSubmit = g.Max(x => x.LastSubmit)
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.TotalTime)
            .ThenBy(x => x.LastSubmit)
            .ToList();

        // 🔥 Step 3: Pagination + Rank
        var totalRecords = grouped.Count;

        var entries = grouped
            .Skip(skip)
            .Take(take)
            .Select((x, index) => new CompetitionLeaderboardQueryModel
            {
                UserId = x.UserId,
                Score = x.Score,
                TotalTime = x.TotalTime,
                LastSubmit = x.LastSubmit,
                Status = UserCompetitionStatus.ACTIVE,

                // Rank theo toàn cục (có pagination)
                Rank = skip + index + 1
            })
            .ToList();

        return (totalRecords, entries);
    }

    private IQueryable<UserRound> ApplyParticipantFilters(
        Guid roundId,
        DateTime? participantStartedFrom,
        DateTime? participantStartedEnd,
        DateTime? participationSubmittedFrom,
        DateTime? participationSubmittedEnd,
        UserRoundStatus? participantStatus,
        bool? isPassed,
        IReadOnlyCollection<Guid>? userIds)
    {
        var query = _context.UserRounds
            .AsNoTracking()
            .Where(ur => ur.RoundID == roundId);

        if (participantStartedFrom.HasValue)
            query = query.Where(ur => ur.StartedAt >= participantStartedFrom.Value);

        if (participantStartedEnd.HasValue)
            query = query.Where(ur => ur.StartedAt <= participantStartedEnd.Value);

        if (participationSubmittedFrom.HasValue)
            query = query.Where(ur => ur.SubmittedAt.HasValue && ur.SubmittedAt.Value >= participationSubmittedFrom.Value);

        if (participationSubmittedEnd.HasValue)
            query = query.Where(ur => ur.SubmittedAt.HasValue && ur.SubmittedAt.Value <= participationSubmittedEnd.Value);

        if (participantStatus.HasValue)
            query = query.Where(ur => ur.Status == participantStatus.Value);

        if (isPassed.HasValue)
            query = query.Where(ur => ur.IsPassed == isPassed.Value);

        if (userIds is { Count: > 0 })
            query = query.Where(ur => userIds.Contains(ur.UserID));

        return query;
    }

    private IQueryable<UserRound> ApplyRoundResultFilters(Guid roundId, UserRoundStatus? status)
    {
        var query = _context.UserRounds
            .AsNoTracking()
            .Where(ur => ur.RoundID == roundId);

        if (status.HasValue)
            query = query.Where(ur => ur.Status == status.Value);

        return query;
    }

    private static IQueryable<UserRound> ApplyRoundResultSorting(
        IQueryable<UserRound> query,
        RoundResultAllSortBy? sortBy,
        SortDirection? sortDirection)
    {
        var direction = sortDirection ?? SortDirection.Desc;
        var sort = sortBy ?? RoundResultAllSortBy.StartedAt;

        return (sort, direction) switch
        {
            (RoundResultAllSortBy.Point, SortDirection.Asc) => query.OrderBy(ur => ur.Point ?? decimal.MinValue).ThenByDescending(ur => ur.StartedAt),
            (RoundResultAllSortBy.Point, _) => query.OrderByDescending(ur => ur.Point ?? decimal.MinValue).ThenByDescending(ur => ur.StartedAt),

            (RoundResultAllSortBy.ExecutionTime, SortDirection.Asc) => query.OrderBy(ur => ur.ExecutionTime ?? TimeSpan.MaxValue).ThenByDescending(ur => ur.StartedAt),
            (RoundResultAllSortBy.ExecutionTime, _) => query.OrderByDescending(ur => ur.ExecutionTime ?? TimeSpan.Zero).ThenByDescending(ur => ur.StartedAt),

            (RoundResultAllSortBy.StartedAt, SortDirection.Asc) => query.OrderBy(ur => ur.StartedAt),
            _ => query.OrderByDescending(ur => ur.StartedAt)
        };
    }

    private IQueryable<UserRound> ApplyMyRoundFilters(
        Guid userId,
        UserRoundStatus? userRoundStatus,
        RoundStatus? roundStatus,
        bool? isPassed)
    {
        var query = _context.UserRounds
            .AsNoTracking()
            .Where(ur => ur.UserID == userId);

        if (userRoundStatus.HasValue)
            query = query.Where(ur => ur.Status == userRoundStatus.Value);

        if (roundStatus.HasValue)
            query = query.Where(ur => ur.Round.Status == roundStatus.Value);

        if (isPassed.HasValue)
            query = query.Where(ur => ur.IsPassed == isPassed.Value);

        return query;
    }
}
