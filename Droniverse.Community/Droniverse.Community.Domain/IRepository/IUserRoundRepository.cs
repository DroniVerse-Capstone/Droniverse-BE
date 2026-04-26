using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Domain.Enums.SeachRequest;
using Droniverse.Community.Infrastructure.QueryModels;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Domain.IRepository;

public interface IUserRoundRepository : IRepository<UserRound>
{
    Task<bool> IsUserJoinedRound(Guid userId, Guid roundId);

    Task<bool> IsUserPassedRound(Guid userId, Guid roundId);

    Task<UserRoundDetailQueryModel?> GetRoundResultByUser(Guid userId, Guid roundId);

    Task<int> CountRoundResults(Guid roundId, UserRoundStatus? status);

    Task DisqualifyByCompetitionAsync(Guid competitionId, Guid userId, DateTime now);

    Task<IEnumerable<RoundResultParticipantQueryModel>> GetRoundResults(
        Guid roundId,
        UserRoundStatus? status,
        RoundResultAllSortBy? sortBy,
        SortDirection? sortDirection,
        int skip,
        int take);

    Task<int> CountMyRounds(
        Guid userId,
        UserRoundStatus? userRoundStatus,
        RoundStatus? roundStatus,
        bool? isPassed);

    Task<IEnumerable<MyRoundQueryModel>> GetMyRounds(
        Guid userId,
        UserRoundStatus? userRoundStatus,
        RoundStatus? roundStatus,
        bool? isPassed,
        int skip,
        int take);

    Task<int> CountRoundParticipants(
        Guid roundId,
        DateTime? participantStartedFrom,
        DateTime? participantStartedEnd,
        DateTime? participationSubmittedFrom,
        DateTime? participationSubmittedEnd,
        UserRoundStatus? participantStatus,
        bool? isPassed,
        IReadOnlyCollection<Guid>? userIds);

    Task<IEnumerable<RoundParticipantQueryModel>> GetRoundParticipants(
        Guid roundId,
        DateTime? participantStartedFrom,
        DateTime? participantStartedEnd,
        DateTime? participationSubmittedFrom,
        DateTime? participationSubmittedEnd,
        UserRoundStatus? participantStatus,
        bool? isPassed,
        IReadOnlyCollection<Guid>? userIds,
        int skip,
        int take);


    Task<(int TotalRecords, IEnumerable<CompetitionLeaderboardQueryModel> Entries)> GetCompetitionLeaderboard(
      Guid competitionId,
      int skip,
      int take);
}
