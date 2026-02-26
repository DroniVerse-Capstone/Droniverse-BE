using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class Participation
{
    public Guid ParticipationID { get; set; }
    public Guid UserID { get; set; } // reference to UserID
    public Guid ApproverID { get; set; } // reference to UserID who approve
    public Club Club { get; set; }
    public Guid ClubID { get; set; } // reference to ClubID
    public ParticipationStatus Status { get; set; } //varchar(50)
    public DateTime JoinDate { get; set; }
}
