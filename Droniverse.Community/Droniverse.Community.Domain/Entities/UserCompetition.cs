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

    public UserCompetition(Guid userID, Guid competitionID)
    {
        UserCompetitionID = Guid.NewGuid();
        UserID = userID;
        CompetitionID = competitionID;

        Status = UserCompetitionStatus.ACTIVE;

        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateScore(decimal score)
    {
        Score = score;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRank(int rank)
    {
        Rank = rank;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignPrize(Guid prizeID)
    {
        PrizeID = prizeID;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Disqualify()
    {
        if (Status == UserCompetitionStatus.DISQUALIFIED)
            throw new InvalidOperationException("Người dùng đã bị loại khỏi cuộc thi.");

        Status = UserCompetitionStatus.DISQUALIFIED;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Withdraw()
    {
        if (Status == UserCompetitionStatus.WITHDRAWN)
            throw new InvalidOperationException("Người dùng đã rút khỏi cuộc thi.");

        Status = UserCompetitionStatus.WITHDRAWN;
        UpdatedAt = DateTime.UtcNow;
    }
}