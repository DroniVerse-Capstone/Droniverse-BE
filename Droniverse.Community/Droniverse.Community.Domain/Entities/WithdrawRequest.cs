using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Domain.Entities;

public class WithdrawRequest
{
    public Guid WithdrawRequestID { get; set; }
    public Guid RequesterID { get; set; }
    public Guid? ApproverID { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Note { get; set; }
    public decimal Amount { get; set; }
    public WithdrawStatus Status { get; set; }
    public string? RejectReason { get; set; }
    public Guid WalletID { get; set; }
    public virtual Wallet Wallet { get; set; }
    public virtual Transaction Transaction { get; set; }

    private WithdrawRequest() { }

    public WithdrawRequest(
        Guid requesterId,
        string? note,
        decimal amount,
        Guid walletId)
    {
        RequesterID = requesterId;
        CreatedAt = DateTime.UtcNow.AddHours(7);
        Note = note;
        Amount = amount;
        Status = WithdrawStatus.PENDING;
        WalletID = walletId;
    }

    public void UpdateStatus(WithdrawStatus status)
    {
        Status = status;
        UpdatedAt = DateTime.UtcNow.AddHours(7);
    }

}
