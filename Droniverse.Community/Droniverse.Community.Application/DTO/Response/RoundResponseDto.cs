using Droniverse.Community.Domain.Enums;

using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundResponseDto
    {
        public Guid RoundID { get; set; }
        public required SimpleCompetitionResponse Competition { get; set; }
        public required SimpleLabResponse Lab { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public RoundStatus RoundStatus { get; set; }
        public RoundLifeCycleStatus? RoundPhase { get; set; }
        public int TotalParticipants { get; set; }
    }
}
