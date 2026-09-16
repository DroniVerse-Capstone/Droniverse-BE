using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class RoundParticipantQueryModel
    {
        public Guid UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public UserRoundStatus Status { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool? IsPassed { get; set; }
    }
}
