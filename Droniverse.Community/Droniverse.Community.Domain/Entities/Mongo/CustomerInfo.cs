namespace Droniverse.Community.Domain.Entities.Mongo;
public class CustomerInfo
{
    public Guid UserID { get; set; }
    public string Name { get; set; } = null!;
    public string? TaxCode { get; set; }
}

