using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;
public class UserRound
{
    public Guid UserRoundID { get; private set; }
    public Guid UserID { get; private set; }
    public Guid RoundID { get; private set; }
    public Round Round { get; private set; }
    public string? Solution { get; private set; }

    // DB: time → nullable
    public TimeSpan? ExecutionTime { get; private set; }
    public int? NumberOfSteps { get; private set; }
    public double? PathLength { get; private set; }
    public string? FeedbackVN { get; private set; }
    public string? FeedbackEN { get; private set; }
    public int? Rating { get; private set; }
    public decimal? Point { get; private set; }
    public bool? IsPassed { get; private set; }
    public bool? IsSequentialCheckpoints { get; private set; }
    public UserRoundStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime StartedAt { get; private set; }
    public int? Rank { get; private set; }
    private UserRound() { }
    public DateTime GetDeadline(TimeSpan duration) => StartedAt + duration;
    public int GetRemainingSeconds(TimeSpan t, DateTime now)
    {
        var remaining = (GetDeadline(t) - now).TotalSeconds;
        return remaining > 0 ? (int)remaining : 0;
    }
    /// <summary>
    /// Khởi tạo record user tham gia vòng thi
    /// </summary>
    public UserRound(Guid userId, Guid roundId, DateTime startedAt)
    {
        UserRoundID = Guid.NewGuid();
        UserID = userId;
        RoundID = roundId;
        Status = UserRoundStatus.InProgress;
        StartedAt = startedAt;
    }

    #region Actions

    public void SubmitSolution(string solution, DateTime now)
    {
        ValidateSubmit();
        Solution = solution;
        SubmittedAt = now;
    }

    public DateTime GetEffectiveSubmittedAt(TimeSpan t, DateTime now)
    {
        var deadline = GetDeadline(t);
        return now > deadline ? deadline : now;
    }

    public void Complete(
        string solution,
        TimeSpan executionTime,
        int steps,
        decimal point,
        bool isPassed,
        bool isSequentialCheckpoints,
        TimeSpan t,
        DateTime now,
        string? feedbackVN = null,
        string? feedbackEN = null)
    {
        ValidateComplete();
        Solution = solution;
        ExecutionTime = executionTime;
        NumberOfSteps = steps;
        Point = point;
        IsPassed = isPassed;
        IsSequentialCheckpoints = isSequentialCheckpoints;
        FeedbackVN = feedbackVN;
        FeedbackEN = feedbackEN;
        Status = UserRoundStatus.Completed;
        SubmittedAt = GetEffectiveSubmittedAt(t, now);
    }

    public void Complete(
        TimeSpan executionTime,
        int steps,
        double pathLength,
        decimal point,
        DateTime now,
        string? feedbackVN = null,
        string? feedbackEN = null)
    {
        ValidateComplete();
        ExecutionTime = executionTime;
        NumberOfSteps = steps;
        PathLength = pathLength;
        Point = point;
        FeedbackVN = feedbackVN;
        FeedbackEN = feedbackEN;
        Status = UserRoundStatus.Completed;
        SubmittedAt = SubmittedAt ?? now;
    }

    public void Disqualify(DateTime now)
    {
        ValidateDisqualify();
        Status = UserRoundStatus.Disqualified;
        SubmittedAt = now;
    }

    public void SetRank(int rank)
    {
        if (rank <= 0)
            throw new InvalidOperationException("Thứ hạng phải lớn hơn 0.");

        Rank = rank;
    }

    #endregion

    #region Validation

    /// <summary>
    /// Kiểm tra xem user có thể submit solution hay không
    /// </summary>
    private void ValidateSubmit()
    {
        if (Status != UserRoundStatus.InProgress)
            throw new InvalidOperationException("Không thể submit, user không đang trong tiến trình thi.");
    }

    /// <summary>
    /// Kiểm tra xem user có thể hoàn thành vòng hay không
    /// </summary>
    private void ValidateComplete()
    {
        if (Status != UserRoundStatus.InProgress)
            throw new InvalidOperationException("Không thể complete, user không đang trong tiến trình thi.");
    }

    /// <summary>
    /// Kiểm tra xem user có thể bị loại hay không
    /// </summary>
    private void ValidateDisqualify()
    {
        if (Status == UserRoundStatus.Disqualified)
            throw new InvalidOperationException("User đã bị loại trước đó.");
    }

    #endregion

    public bool IsFinished() => Status == UserRoundStatus.Completed || Status == UserRoundStatus.Disqualified;
}