namespace Droniverse.Community.Domain.Entities;
public class UserProduct
{
    public Guid UserProductID { get; set; }
    public Product Product { get; set; }
    public Guid ProductID { get; set; }
    public Guid UserID { get; set; }
    public DateTime AcquiredAt { get; set; }
    public string Source { get; set; }
    public string Status { get; set; }

    // Foreign Keys
}
