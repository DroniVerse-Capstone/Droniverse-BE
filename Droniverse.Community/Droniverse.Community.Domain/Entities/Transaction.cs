using Droniverse.Community.Domain.Enums;
using System.Transactions;

namespace Droniverse.Community.Domain.Entities;

public class Transaction
{
    public Guid TransactionID { get; set; }
    public Guid WalletID { get; set; }
    public Wallet Wallet { get; set; }
    public int Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatusEnum Status { get; set; }
    public Guid ReferenceID { get; set; }
    public DateTime CreatedAt { get; set; }

    private Transaction() { }

    public Transaction(
        Guid walletId,
        int amount,
        TransactionType type,
        TransactionStatusEnum status,
        Guid referenceID
      )
    {
        WalletID = walletId;
        Amount = amount;
        Type = type;
        Status = status;
        ReferenceID = referenceID;
    }

}
