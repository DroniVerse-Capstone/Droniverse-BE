using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundLeaderboardEntryDto
    {
        public SimpleUserReponse User { get; set; }

        // Ranking metrics
        public decimal Point { get; set; }
        public TimeSpan ExecutionTime { get; set; }

        public bool IsPassed { get; set; }
        public UserRoundStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int Rank { get; set; }

        public bool IsCurrentUser { get; set; }
    }
}
