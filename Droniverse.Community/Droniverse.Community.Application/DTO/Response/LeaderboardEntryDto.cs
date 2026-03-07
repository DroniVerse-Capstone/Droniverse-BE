using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class LeaderboardEntryDto
    {
        public Guid UserID { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public UserCompetitionStatus Status { get; set; }
    }
}
