using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundLeaderboardEntryDto
    {
        public SimpleUserReponse User { get; set; }
        public decimal Point { get; set; }
        public double ExecutionTime { get; set; }
        public int NumberOfSteps { get; set; }
        public double PathLength { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public int Rank { get; set; }
    }
}
