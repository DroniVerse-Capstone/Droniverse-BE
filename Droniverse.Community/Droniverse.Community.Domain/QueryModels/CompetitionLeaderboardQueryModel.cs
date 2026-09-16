using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class CompetitionLeaderboardQueryModel
    {
        public Guid UserId { get; set; }
        public decimal Score { get; set; }
        public int? Rank { get; set; }
        public UserCompetitionStatus Status { get; set; }
        public TimeSpan TotalTime { get; set; }
        public DateTime? LastSubmit { get; set; }
    }
}
