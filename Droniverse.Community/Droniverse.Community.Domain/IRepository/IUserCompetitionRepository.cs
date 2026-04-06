using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
using Droniverse.Community.Infrastructure.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Domain.IRepository
{
    public interface IUserCompetitionRepository : IRepository<UserCompetition>
    {
        Task<Dictionary<Guid, int>> GetCompetitorCountsByCompetitionIds(IEnumerable<Guid> competitionIds);

        Task<(int TotalRecords, IEnumerable<CompetitionLeaderboardQueryModel> Entries)> GetCompetitionLeaderboard(
            Guid competitionId,
            int skip,
            int take);

        Task<(int TotalRecords, IEnumerable<CompetitionParticipantQueryModel> Participants)> GetCompetitionParticipants(
            Guid competitionId,
            UserCompetitionStatus status,
            DateTime? joinFrom,
            int skip,
            int take);
    }
}
