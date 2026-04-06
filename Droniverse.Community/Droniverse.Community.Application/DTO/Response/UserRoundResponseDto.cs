using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class UserRoundResponseDto
    {
        public Guid UserRoundID { get; set; }
        public UserRoundStatus Status { get; set; }
        public required string Solution { get; set; }
        public decimal Point { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int NumberOfSteps { get; set; }
        public double PathLength { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsPassed { get; set; }
        public int? Rank { get; set; }
        public int? Rating { get; set; }
        public string? FeedbackVN { get; set; }
        public string? FeedbackEN { get; set; }
        public required SimpleRoundResponse Round { get; set; }
    }
}
