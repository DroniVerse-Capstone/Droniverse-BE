namespace Droniverse.Academy.Application.DTO.Response;

public class ProductMiniResponseDTO
{
    public Guid ProductId { get; set; }
    public Guid ReferenceId { get; set; }
    public decimal Price { get; set; }
    public CurrencyType Currency { get; set; }
    public ProductStatus Status { get; set; }
}
