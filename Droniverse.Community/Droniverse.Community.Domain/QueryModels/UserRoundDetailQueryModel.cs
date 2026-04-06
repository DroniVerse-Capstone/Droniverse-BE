using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Infrastructure.QueryModels
{
    public class UserRoundDetailQueryModel
    {
        public Guid UserRoundId { get; set; }
        public UserRoundStatus Status { get; set; }
        public string? Solution { get; set; }
        public decimal? Point { get; set; }
        public TimeSpan? ExecutionTime { get; set; }
        public int? NumberOfSteps { get; set; }
        public double? PathLength { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool? IsPassed { get; set; }
        public int? Rank { get; set; }
        public int? Rating { get; set; }
        public string? FeedbackVN { get; set; }
        public string? FeedbackEN { get; set; }

        public Guid RoundId { get; set; }
        public int RoundNumber { get; set; }
        public DateTime RoundStartTime { get; set; }
        public DateTime RoundEndTime { get; set; }
        public TimeSpan RoundTimeLimit { get; set; }
        public RoundStatus RoundStatus { get; set; }
    }
}
