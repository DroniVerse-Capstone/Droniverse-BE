using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Round
{
    public Guid RoundID { get; private set; }

    public Guid CompetitionID { get; private set; }
    public Competition? Competition { get; private set; }

    public Guid LabID { get; private set; }

    public int RoundNumber { get; private set; }

    public DateTime StartTime { get; private set; }

    public DateTime EndTime { get; private set; }

    public RoundStatus Status { get; private set; }

    public ICollection<UserRound> UserRounds { get; private set; } = new List<UserRound>();

    private Round() { }

    public Round(Guid competitionId, Guid labId, int roundNumber, DateTime startTime, DateTime endTime)
    {
        if (startTime >= endTime)
            throw new ArgumentException("Thời gian bắt đầu phải trước thời gian kết thúc.");

        RoundID = Guid.NewGuid();
        CompetitionID = competitionId;
        LabID = labId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Pending;
    }

    public void StartRound(DateTime now)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể bắt đầu vòng khi đang ở trạng thái chờ.");

        if (now < StartTime)
            throw new InvalidOperationException("Không thể bắt đầu vòng trước thời gian bắt đầu.");

        Status = RoundStatus.Ongoing;
    }

    public void FinishRound(DateTime now)
    {
        if (Status != RoundStatus.Ongoing)
            throw new InvalidOperationException("Chỉ có thể kết thúc vòng khi đang diễn ra.");

        if (now < EndTime)
            throw new InvalidOperationException("Không thể kết thúc vòng trước thời gian kết thúc.");

        Status = RoundStatus.Finished;
    }

    public void MarkAsScheduleInvalid()
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể đánh dấu lịch không hợp lệ khi vòng đang ở trạng thái chờ.");

        Status = RoundStatus.SCHEDULE_INVALID;
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
}