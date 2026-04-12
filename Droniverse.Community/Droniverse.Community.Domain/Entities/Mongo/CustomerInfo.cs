namespace Droniverse.Community.Domain.Entities.Mongo;
public class CustomerInfo
{
    public Guid UserID { get; set; }
    public required string FullName { get; set; } = null!;
    public required string Email { get; set; } = null!;
    public string? TaxCode { get; set; }
}

