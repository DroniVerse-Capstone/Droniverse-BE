using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class UserCompetition
{
    public Guid UserCompetitionID { get; private set; }
    public Guid UserID { get; private set; }
    public Guid CompetitionID { get; private set; }
    public Competition Competition { get; private set; }
    public UserCompetitionStatus Status { get; private set; }
    public decimal? Score { get; private set; }
    public int? Rank { get; private set; }
    public Guid? PrizeID { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    private UserCompetition() { }

    public UserCompetition(Guid userID, Guid competitionID, DateTime now)
    {
        UserCompetitionID = Guid.NewGuid();
        UserID = userID;
        CompetitionID = competitionID;

        Status = UserCompetitionStatus.ACTIVE;

        CreatedAt = now;
    }

    public void UpdateScore(decimal score, DateTime now)
    {
        Score = score;
        UpdatedAt = now;
    }

    public void UpdateRank(int rank, DateTime now)
    {
        Rank = rank;
        UpdatedAt = now;
    }

    public void AssignPrize(Guid prizeID, DateTime now)
    {
        PrizeID = prizeID;
        UpdatedAt = now;
    }

    public void Rejoin(DateTime now)
    {
        if (Status != UserCompetitionStatus.WITHDRAWN)
            throw new InvalidOperationException("Chỉ có thể tham gia lại khi đã rút lui.");

        Status = UserCompetitionStatus.ACTIVE;

        // reset dữ liệu (tuỳ business)
        Score = null;
        Rank = null;
        PrizeID = null;

        UpdatedAt = now;
    }

    public void Disqualify(DateTime now)
    {
        if (Status == UserCompetitionStatus.DISQUALIFIED)
            throw new InvalidOperationException("Người dùng đã bị loại khỏi cuộc thi.");

        Status = UserCompetitionStatus.DISQUALIFIED;
        UpdatedAt = now;
    }

    public void Withdraw(DateTime now)
    {
        if (Status == UserCompetitionStatus.WITHDRAWN)
            throw new InvalidOperationException("Người dùng đã rút khỏi cuộc thi.");

        Status = UserCompetitionStatus.WITHDRAWN;
        UpdatedAt = now;
    }
}