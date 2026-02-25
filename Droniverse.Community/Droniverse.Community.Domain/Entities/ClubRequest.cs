namespace Droniverse.Community.Domain.Entities;
public class ClubRequest
{
    public Guid ClubRequestID { get; set; } 
    public Guid RequesterID { get; set; } // Reference to UserID
    public Guid ApproverID { get; set; } // Reference to UserID
    public Club Club { get; set; }
    public Guid ClubID { get; set; } // Reference to ClubID
}
