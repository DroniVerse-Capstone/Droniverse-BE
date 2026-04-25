using Droniverse.Community.Domain.Entities;
using Droniverse.Community.Domain.Enums;

public class UserRound
{
    public Guid UserRoundID { get; private set; }
    public Guid UserID { get; private set; }
    public Guid RoundID { get; private set; }
    public Round Round { get; private set; }

    public TimeSpan? ExecutionTime { get; private set; }
    public decimal? Point { get; private set; }
    public bool? IsPassed { get; private set; }
    public UserRoundStatus Status { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public DateTime StartedAt { get; private set; }
    public int? Rank { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private UserRound() { }

    public UserRound(Guid userId, Guid roundId, DateTime startedAt)
    {
        UserRoundID = Guid.NewGuid();
        UserID = userId;
        RoundID = roundId;
        Status = UserRoundStatus.InProgress;
        StartedAt = startedAt;
        UpdatedAt = startedAt;
    }

    #region Time Helpers

    public DateTime GetDeadline(TimeSpan duration, DateTime roundEndTime)
    {
        var deadline = StartedAt + duration;
        return deadline > roundEndTime ? roundEndTime : deadline;
    }

    public int GetRemainingSeconds(TimeSpan t, DateTime now, DateTime roundEndTime)
    {
        var remaining = (GetDeadline(t, roundEndTime) - now).TotalSeconds;
        return remaining > 0 ? (int)remaining : 0;
    }

    public DateTime GetEffectiveSubmittedAt(TimeSpan t, DateTime now, DateTime roundEndTime)
    {
        var deadline = GetDeadline(t, roundEndTime);
        return now > deadline ? deadline : now;
    }

    #endregion

    #region Actions

    public void SubmitSolution(string solution, DateTime now)
    {
        ValidateSubmit();
        SubmittedAt = now;
        UpdatedAt = now;
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
        DateTime roundEndTime)
    {
        ValidateComplete();

        ExecutionTime = executionTime;
        Point = point;
        IsPassed = isPassed;
        Status = UserRoundStatus.Completed;
        SubmittedAt = GetEffectiveSubmittedAt(t, now, roundEndTime);
        UpdatedAt = now;
    }

    public void Complete(
        TimeSpan executionTime,
        int steps,
        double pathLength,
        decimal point,
        DateTime now,
        TimeSpan t,
        DateTime roundEndTime)
    {
        ValidateComplete();

        ExecutionTime = executionTime;
        Point = point;
        Status = UserRoundStatus.Completed;
        SubmittedAt = GetEffectiveSubmittedAt(t, now, roundEndTime);
        UpdatedAt = now;
    }

    public void Disqualify(DateTime now)
    {
        ValidateDisqualify();
        Status = UserRoundStatus.Disqualified;
        SubmittedAt = now;
        UpdatedAt = now;
    }

    // ⭐ dùng cho withdraw (khuyên dùng)
    public void DisqualifyDueToWithdraw(DateTime now)
    {
        if (Status == UserRoundStatus.Disqualified)
            return;

        Status = UserRoundStatus.Disqualified;
        SubmittedAt = now;

        ExecutionTime = null;
        Point = null;
        IsPassed = null;
        Rank = null;

        UpdatedAt = now;
    }

    public void SetRank(int rank, DateTime now)
    {
        if (rank <= 0)
            throw new InvalidOperationException("Thứ hạng phải lớn hơn 0.");

        Rank = rank;
        UpdatedAt = now;
    }

    #endregion

    #region Validation

    private void ValidateSubmit()
    {
        if (Status != UserRoundStatus.InProgress)
            throw new InvalidOperationException("Không thể submit, user không đang trong tiến trình thi.");
    }

    private void ValidateComplete()
    {
        if (Status != UserRoundStatus.InProgress)
            throw new InvalidOperationException("Không thể complete, user không đang trong tiến trình thi.");
    }

    private void ValidateDisqualify()
    {
        if (Status == UserRoundStatus.Disqualified)
            throw new InvalidOperationException("User đã bị loại trước đó.");
    }

    #endregion

    public bool IsFinished() =>
        Status == UserRoundStatus.Completed ||
        Status == UserRoundStatus.Disqualified;
}