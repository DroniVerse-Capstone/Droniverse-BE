using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class RoundResponseDto
    {
        public Guid RoundID { get; set; }
        public Guid CompetitionID { get; set; }
        public Guid LabID { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public RoundStatus Status { get; set; }
        public int TotalParticipants { get; set; }
    }
}
