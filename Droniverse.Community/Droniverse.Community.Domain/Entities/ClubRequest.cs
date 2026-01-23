namespace Droniverse.Community.Domain.Entities;
public class ClubRequest
{
    public Guid ClubRequestID { get; set; }
    public Guid RequestID { get; set; } // Reference to UserID
    public Guid ApproveID { get; set; } // Reference to UserID
    public Club Club { get; set; }
    public Guid ClubID { get; set; } // Reference to ClubID
}
