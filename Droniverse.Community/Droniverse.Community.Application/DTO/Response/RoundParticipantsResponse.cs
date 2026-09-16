using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public record RoundParticipantsResponse
    {
        public Guid RoundID { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public RoundStatus Status { get; set; }
        public required List<RoundParticipantsEntryResponse> participants { get; set; }
    }
}
