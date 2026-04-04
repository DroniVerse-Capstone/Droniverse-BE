namespace Droniverse.Community.Application.DTO.Response
{
    public class UserRoundResponseDto
    {
        public Guid UserRoundID { get; set; }
        public Guid UserID { get; set; }
        public Guid RoundID { get; set; }
        public string? Solution { get; set; }
        public bool IsCompleted { get; set; }
        public double ExecutionTime { get; set; }
        public int NumberOfSteps { get; set; }
        public double PathLength { get; set; }
        public string? FeedbackVN { get; set; }
        public string? FeedbackEN { get; set; }
        public int Rating { get; set; }
        public decimal Point { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }
}
