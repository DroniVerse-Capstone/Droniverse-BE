using Droniverse.Community.Domain.Entities;

public class UserRound
{
    public Guid UserRoundID { get; private set; }

    public Guid UserID { get; private set; }

    public Guid RoundID { get; private set; }
    public Round? Round { get; private set; }

    public string? Solution { get; private set; }

    public bool IsCompleted { get; private set; }

    public double ExecutionTime { get; private set; }

    public int NumberOfSteps { get; private set; }

    public double PathLength { get; private set; }

    public string? FeedbackVN { get; private set; }

    public string? FeedbackEN { get; private set; }

    public int Rating { get; private set; }

    public decimal Point { get; private set; }

    public DateTime? SubmittedAt { get; private set; }

    private UserRound() { }

    public UserRound(Guid userId, Guid roundId)
    {
        UserRoundID = Guid.NewGuid();
        UserID = userId;
        RoundID = roundId;
        IsCompleted = false;
    }

    public void SubmitSolution(string solution)
    {
        Solution = solution;
    }

    public void Complete(
        double executionTime,
        int steps,
        double pathLength,
        decimal point,
        string? feedbackVN,
        string? feedbackEN)
    {
        ExecutionTime = executionTime;
        NumberOfSteps = steps;
        PathLength = pathLength;
        Point = point;
        FeedbackVN = feedbackVN;
        FeedbackEN = feedbackEN;
        IsCompleted = true;
        SubmittedAt = DateTime.UtcNow;
    }
}