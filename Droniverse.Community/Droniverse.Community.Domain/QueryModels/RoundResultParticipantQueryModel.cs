using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class RoundResultParticipantQueryModel
    {
        public Guid UserId { get; set; }
        public UserRoundStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool? IsPassed { get; set; }
        public int? Rank { get; set; }
    }
}
