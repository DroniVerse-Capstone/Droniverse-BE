namespace Droniverse.Community.Domain.Entities;
public class Wallet
{
    public Guid WalletID { get; set; }
    public Guid OwnerID { get; set; }
    public string BankNumber { get; set; }
    public string Bank {  get; set; }
    public decimal Balance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Transaction> Transactions { get; set; }

}
