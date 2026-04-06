using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public class LeaderboardEntryDto
    {
        public required SimpleUserReponse User { get; set; }
        public decimal? Score { get; set; }
        public int? Rank { get; set; }
        public UserCompetitionStatus Status { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}
