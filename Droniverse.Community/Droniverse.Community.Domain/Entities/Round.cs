using Droniverse.Community.Domain.Enums;

using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Round
{
    public Guid RoundID { get; private set; }
    public Guid CompetitionID { get; private set; }
    public Competition Competition { get; private set; }
    public Guid LabID { get; private set; }
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
        LabID = labId;
        RoundNumber = roundNumber;
        TimeLimit = timeLimit;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Pending;
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
        LabID = labId;
        RoundNumber = roundNumber;
        TimeLimit = timeLimit;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Pending;
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

    public void StartRound(DateTime now, Guid? updatedBy)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể bắt đầu vòng khi đang ở trạng thái chờ.");

        if (now < StartTime)
            throw new InvalidOperationException("Không thể bắt đầu vòng trước thời gian bắt đầu.");

        Status = RoundStatus.Ongoing;
        SetUpdated(now, updatedBy);
    }

    public void FinishRound(DateTime now)
    {
        FinishRound(now, null);
    }

    public void FinishRound(DateTime now, Guid? updatedBy)
    {
        if (Status != RoundStatus.Ongoing)
            throw new InvalidOperationException("Chỉ có thể kết thúc vòng khi đang diễn ra.");

        if (now < EndTime)
            throw new InvalidOperationException("Không thể kết thúc vòng trước thời gian kết thúc.");

        Status = RoundStatus.Finished;
        IsSummarized = false;
        SetUpdated(now, updatedBy);
    }

    public void MarkAsScheduleInvalid()
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể đánh dấu lịch không hợp lệ khi vòng đang ở trạng thái chờ.");

        Status = RoundStatus.SCHEDULE_INVALID;
    }

    public void MarkAsScheduleInvalid(DateTime now, Guid? updatedBy = null)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể đánh dấu lịch không hợp lệ khi vòng đang ở trạng thái chờ.");

        Status = RoundStatus.SCHEDULE_INVALID;
        SetUpdated(now, updatedBy);
    }

    public void UpdateInfo(Guid labId, int roundNumber, DateTime startTime, DateTime endTime)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể cập nhật khi vòng đang ở trạng thái chờ.");

        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        LabID = labId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        EndTime = endTime;
        IsSummarized = false;
    }

    public void UpdateInfo(Guid labId, int roundNumber, DateTime startTime, DateTime endTime, DateTime now, Guid? updatedBy)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể cập nhật khi vòng đang ở trạng thái chờ.");

        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        LabID = labId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        EndTime = endTime;
        IsSummarized = false;
        SetUpdated(now, updatedBy);
    }

    public void MarkSummarized(DateTime now, Guid? updatedBy = null)
    {
        if (Status != RoundStatus.Finished)
            throw new InvalidOperationException("Chỉ có thể tổng hợp khi vòng thi đã kết thúc.");

        IsSummarized = true;
        SetUpdated(now, updatedBy);
    }

    public bool IsScheduleInvalid(DateTime now)
    {
        return Status == RoundStatus.Pending && now > EndTime;
    }

    public bool IsActive(DateTime now)
    {
        return Status == RoundStatus.Ongoing && now >= StartTime && now <= EndTime;
    }

    public bool CanStart(DateTime now, bool isPreviousRoundFinished, CompetitionStatus competitionStatus)
    {
        return Status == RoundStatus.Pending
            && now >= StartTime
            && isPreviousRoundFinished
            && competitionStatus == CompetitionStatus.ONGOING;
    }

    public bool CanFinish(DateTime now)
    {
        return Status == RoundStatus.Ongoing
            && now >= EndTime;
    }

    public void ValidateUserCanJoin(DateTime now, bool isPreviousRoundFinished, CompetitionStatus competitionStatus)
    {
        if (Status != RoundStatus.Ongoing)
            throw new InvalidOperationException("Vòng thi chưa diễn ra hoặc đã kết thúc.");

        if (now < StartTime || now > EndTime)
            throw new InvalidOperationException("Thời gian tham gia không hợp lệ.");

        if (competitionStatus != CompetitionStatus.ONGOING)
            throw new InvalidOperationException("Competition hiện tại chưa diễn ra hoặc đã kết thúc.");

        if (!isPreviousRoundFinished)
            throw new InvalidOperationException("Vòng trước chưa hoàn tất, không thể tham gia vòng này.");
    }

    public void ValidateUserCanSubmit(DateTime now)
    {
        if (now < StartTime)
            throw new InvalidOperationException("Vòng thi chưa tới thời gian nộp bài.");
    }
}