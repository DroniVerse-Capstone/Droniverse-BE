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
        RoundID = Guid.NewGuid();
        CompetitionID = competitionId;
        LabID = labId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        EndTime = endTime;
        Status = RoundStatus.Pending;
    }

    public void StartRound()
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Round can only start from Pending state.");

        Status = RoundStatus.Ongoing;
    }

    public void FinishRound()
    {
        if (Status != RoundStatus.Ongoing)
            throw new InvalidOperationException("Round must be Ongoing before finishing.");

        Status = RoundStatus.Finished;
    }

    public void MarkAsScheduleInvalid()
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Chỉ có thể đánh dấu SCHEDULE_INVALID cho round đang ở trạng thái Pending.");

        Status = RoundStatus.SCHEDULE_INVALID;
    }

    public void UpdateInfo(Guid labId, int roundNumber, DateTime startTime, DateTime endTime)
    {
        if (Status != RoundStatus.Pending)
            throw new InvalidOperationException("Only pending rounds can be updated.");

        LabID = labId;
        RoundNumber = roundNumber;
        StartTime = startTime;
        EndTime = endTime;
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return Status == RoundStatus.Ongoing && now >= StartTime && now <= EndTime;
    }
}