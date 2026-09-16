namespace Droniverse.Community.Domain.Entities;

public class Wallet
{
    public Guid WalletID { get; private set; }
    public Guid OwnerID { get; private set; }
    public string BankNumber { get; set; }
    public string Bank { get; set; }
    public decimal Balance { get; private set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Transaction> Transactions { get; set; }

    private Wallet() { }
    public Wallet(
        Guid ownerID,
        string bank,
        string bankNumber,
        DateTime createdAt,
        DateTime updatedAt)
    {
        OwnerID = ownerID;
        Bank = bank;
        BankNumber = bankNumber;
        Balance = 0;
        CreatedAt = createdAt; 
        UpdatedAt = updatedAt;
        WalletID = Guid.NewGuid();
    }

    public void UpdateProfile(string bankNumber, string bank, DateTime updatedAt)
    {
        BankNumber = bankNumber;
        Bank = bank;
        UpdatedAt = updatedAt;
    }

    public void UpdateBalance(decimal amount, DateTime updatedAt)
    {
        if (amount < 0 && Balance + amount < 0)
        {
            throw new InvalidOperationException("Số dư trong ví không đủ để thực hiện giao dịch.");
        }

        Balance += amount;
        UpdatedAt = updatedAt;
    }
}
