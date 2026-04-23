using Droniverse.Community.Domain.Enums;

using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Round
{
    public Guid RoundID { get; private set; }
    public Guid CompetitionID { get; private set; }
    public Competition Competition { get; private set; }
    public Guid VRSimilatorID { get; private set; }
    public int RoundNumber { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public TimeSpan TimeLimit { get; private set; }
    public RoundStatus Status { get; private set; }

    // Flag kiểm tra round đã được tổng hợp leaderboard chưa
    public bool IsSummarized { get; private set; }

    // --- Metadata ---
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public ICollection<UserRound> UserRounds { get; private set; } = [];
    private Round() { }
    public Round(
        Guid competitionId,
        Guid labId,
        int roundNumber,
        DateTime startTime,
        DateTime endTime,
        TimeSpan timeLimit,
        DateTime createdAt,
        Guid createdBy)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        RoundID = Guid.NewGuid();
        CompetitionID = competitionId;
        VRSimilatorID = labId;
        RoundNumber = roundNumber;
        TimeLimit = timeLimit;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Valid;
        IsSummarized = false;
        CreatedAt = createdAt;
        CreatedBy = createdBy;
    }

    public Round(Guid competitionId, Guid labId, int roundNumber, DateTime startTime, DateTime endTime, TimeSpan timeLimit)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        RoundID = Guid.NewGuid();
        CompetitionID = competitionId;
        VRSimilatorID = labId;
        RoundNumber = roundNumber;
        TimeLimit = timeLimit;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Valid;
        IsSummarized = false;
        CreatedAt = default;
        CreatedBy = Guid.Empty;
    }

    private void SetUpdated(DateTime now, Guid? updatedBy)
    {
        UpdatedAt = now;
        UpdatedBy = updatedBy;
    }

    public void StartRound(DateTime now)
    {
        StartRound(now, null);
    }

    public void CancelRound(DateTime now, Guid userId)
    {
        SetUpdated(now, userId);
        Status = RoundStatus.Cancelled;
    }

    public void StartRound(DateTime now, Guid? updatedBy)
    {
        if (Status != RoundStatus.Valid)
            throw new InvalidOperationException("Chỉ có thể bắt đầu khi round đang ở trạng thái hợp lệ.");

        if (now < StartTime)
            throw new InvalidOperationException("Không thể bắt đầu vòng trước thời gian bắt đầu.");

        SetUpdated(now, updatedBy);
    }

    public void FinishRound(DateTime now)
    {
        FinishRound(now, null);
    }

    public void FinishRound(DateTime now, Guid? updatedBy)
    {
        if (Status != RoundStatus.Valid)
            throw new InvalidOperationException("Chỉ có thể kết thúc khi round đang ở trạng thái hợp lệ.");

        if (now < EndTime)
            throw new InvalidOperationException("Không thể kết thúc vòng trước thời gian kết thúc.");

        IsSummarized = false;
        SetUpdated(now, updatedBy);
    }

    public void MarkAsScheduleInvalid()
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Không thể đánh dấu lỗi lịch cho round đã bị hủy.");

        Status = RoundStatus.ScheduleInvalid;
    }

    public void MarkAsScheduleInvalid(DateTime now, Guid? updatedBy = null)
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Không thể đánh dấu lỗi lịch cho round đã bị hủy.");

        Status = RoundStatus.ScheduleInvalid;
        SetUpdated(now, updatedBy);
    }

    public void RestoreRound(DateTime now, Guid userId)
    {
        Status = RoundStatus.Valid;
        SetUpdated(now, userId);
    }

    public void UpdateInfo(Guid labId, DateTime startTime, DateTime endTime)
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Không thể cập nhật round đã bị hủy.");

        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        VRSimilatorID = labId;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Valid;
        IsSummarized = false;
    }

    public void UpdateInfo(Guid labId, DateTime startTime, DateTime endTime, DateTime now, Guid? updatedBy)
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Không thể cập nhật round đã bị hủy.");

        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        VRSimilatorID = labId;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Valid;
        IsSummarized = false;
        SetUpdated(now, updatedBy);
    }


    public void MarkSummarized(DateTime now, Guid? updatedBy = null)
    {
        if (Status != RoundStatus.Valid)
            throw new InvalidOperationException("Chỉ có thể tổng hợp khi round đang ở trạng thái hợp lệ.");

        if (now < EndTime)
            throw new InvalidOperationException("Chỉ có thể tổng hợp khi vòng thi đã kết thúc.");

        IsSummarized = true;
        SetUpdated(now, updatedBy);
    }

    public bool IsScheduleInvalid(DateTime now)
    {
        return Status == RoundStatus.Valid && now > EndTime && !IsSummarized;
    }

    public bool IsActive(DateTime now)
    {
        return Status == RoundStatus.Valid && now >= StartTime && now <= EndTime;
    }

    public bool CanStart(DateTime now, bool isPreviousRoundFinished, CompetitionStatus competitionStatus)
    {
        return Status == RoundStatus.Valid
            && now >= StartTime
            && isPreviousRoundFinished
            && competitionStatus == CompetitionStatus.PUBLISHED;
    }

    public bool CanFinish(DateTime now)
    {
        return Status == RoundStatus.Valid
            && now >= EndTime;
    }

    public void ValidateUserCanJoin(
        DateTime now,
        bool isUserInCompetition,
        bool isUserJoinedRound,
        bool isUserPassedPreviousRound)
    {
        if (Status != RoundStatus.Valid)
            throw new InvalidOperationException("Vòng thi không hợp lệ để tham gia.");

        if (!isUserInCompetition)
            throw new InvalidOperationException("Người dùng chưa tham gia cuộc thi.");

        if (Competition == null || Competition.Status != CompetitionStatus.PUBLISHED)
            throw new InvalidOperationException("Cuộc thi chưa diễn ra hoặc đã kết thúc.");

        if (now < StartTime || now > EndTime)
            throw new InvalidOperationException("Thời gian tham gia vòng thi không hợp lệ.");

        if (isUserJoinedRound)
            throw new InvalidOperationException("Bạn đã tham gia vòng thi này rồi.");

        if (RoundNumber > 1 && !isUserPassedPreviousRound)
            throw new InvalidOperationException("Bạn chưa vượt qua vòng trước nên không đủ điều kiện tham gia.");
    }

    public void ValidateCanCancel(DateTime now)
    {
        if (Status == RoundStatus.Cancelled)
            throw new InvalidOperationException("Round đã bị hủy trước đó.");

        if (IsSummarized)
            throw new InvalidOperationException("Round đã được tổng hợp, không thể hủy.");

        if (now >= StartTime)
            throw new InvalidOperationException("Round đã bắt đầu, không thể hủy.");
    }

    public void ValidateUserCanSubmit(DateTime now)
    {
        if (Status != RoundStatus.Valid)
            throw new InvalidOperationException("Round không hợp lệ để nộp bài.");

        if (now < StartTime)
            throw new InvalidOperationException("Vòng thi chưa tới thời gian nộp bài.");

        if (now > EndTime)
            throw new InvalidOperationException("Đã quá thời gian nộp bài của vòng thi.");
    }
}