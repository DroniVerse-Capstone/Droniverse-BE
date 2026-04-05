using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class MyRoundQueryModel
    {
        public Guid RoundId { get; set; }
        public int RoundNumber { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TimeLimit { get; set; }
        public RoundStatus RoundStatus { get; set; }
        public UserRoundStatus Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool? IsPassed { get; set; }
        public int? Rank { get; set; }
    }
}
