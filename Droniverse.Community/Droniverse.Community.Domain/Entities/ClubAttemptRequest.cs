using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;
public class ClubAttemptRequest
{
    public Guid ClubRequestID { get; private set; }
    public Guid RequesterID { get; private set; }
    public Guid? ApproverID { get; private set; }
    public Guid ClubID { get; private set; }
    public Club Club { get; private set; }
    public ClubAttemptRequestStatus Status { get; private set; }
    public DateTime CreateAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    private ClubAttemptRequest() { } // For EF

    public ClubAttemptRequest(Guid requesterId, Guid clubId)
    {
        ClubRequestID = Guid.NewGuid();
        RequesterID = requesterId;
        ClubID = clubId;
        Status = ClubAttemptRequestStatus.PENDING;
        CreateAt = DateTime.UtcNow;
    }

    public void Approve(Guid approverId)
    {
        if (Status != ClubAttemptRequestStatus.PENDING)
            throw new InvalidOperationException("Only pending request can be approved.");

        Status = ClubAttemptRequestStatus.APPROVED;
        ApproverID = approverId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(Guid approverId)
    {
        if (Status != ClubAttemptRequestStatus.PENDING)
            throw new InvalidOperationException("Only pending request can be rejected.");

        Status = ClubAttemptRequestStatus.REJECT;
        ApproverID = approverId;
        ProcessedAt = DateTime.UtcNow;
    }

    public void ResetToPending()
    {
        Status = ClubAttemptRequestStatus.PENDING;
        ApproverID = null;
        ProcessedAt = null;
    }
}
