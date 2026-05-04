using Droniverse.Shared.Enums;

namespace Droniverse.Community.Domain.Entities;

public class Participation
{
    public Guid ParticipationID { get; private set; }
    public Guid UserID { get; private set; }
    public Guid ClubID { get; private set; }
    public Guid? ApproverID { get; private set; }
    public Club Club { get; private set; }
    public ParticipationStatus Status { get; private set; }
    public string? Note { get; private set; }
    public DateTime JoinDate { get; private set; }
    public DateTime? LeftDate { get; private set; }

    private Participation() { } // For EF

    public Participation(Guid userId, Guid clubId, Guid? approverId, DateTime joinDate)
    {
        ParticipationID = Guid.NewGuid();
        UserID = userId;
        ClubID = clubId;
        ApproverID = approverId;
        Status = ParticipationStatus.ACTIVE;
        JoinDate = joinDate;
        Note = null;
        LeftDate = null;
    }

    public void Leave(string note, DateTime leftDate)
    {
        if (Status != ParticipationStatus.ACTIVE)
            throw new InvalidOperationException("Only active member can leave.");

        Status = ParticipationStatus.LEFT;
        LeftDate = leftDate;
        Note = note;
    }

    public void Ban(string note, DateTime leftDate)
    {
        if (Status == ParticipationStatus.BANNED)
            throw new InvalidOperationException("Member is already banned.");

        Status = ParticipationStatus.BANNED;
        LeftDate = leftDate;
        Note = note;
    }

    public void Reactivate(Guid approverId, DateTime joinDate)
    {
        if (Status == ParticipationStatus.BANNED)
            throw new InvalidOperationException("Banned member cannot be reactivated.");

        Status = ParticipationStatus.ACTIVE;
        ApproverID = approverId;
        JoinDate = joinDate;
        LeftDate = null;
    }
}