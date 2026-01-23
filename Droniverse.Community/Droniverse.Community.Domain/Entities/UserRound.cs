using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class UserRound
{
    public Guid UserRoundID { get; set; }
    public Guid UserID { get; set; }
    public Round Round { get; set; }
    public Guid RoundID { get; set; }
    public string Solution { get; set; }
    public bool IsCompleted { get; set; }
    public float Time { get; set; }
    public int NumberOfStep { get; set; }
    public float Length { get; set; }
    public string FeedbackVN { get; set; }
    public string FeedbackEN { get; set; }
    public int Rating { get; set; }
    public decimal Point { get; set; }
}

