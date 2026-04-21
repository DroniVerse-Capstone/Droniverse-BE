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
        string bankNumber)
    {
        OwnerID = ownerID;
        Bank = bank;
        BankNumber = bankNumber;
        Balance = 0;
        CreatedAt = DateTime.UtcNow.AddHours(7);
        UpdatedAt = DateTime.UtcNow.AddHours(7);
        WalletID = Guid.NewGuid();
    }

    public void UpdateProfile(string bankNumber, string bank)
    {
        BankNumber = bankNumber;
        Bank = bank;
        UpdatedAt = DateTime.UtcNow.AddHours(7);
    }

}
