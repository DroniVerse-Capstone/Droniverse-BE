namespace Droniverse.Academy.Application.DTO.Response;

public class ProductMiniResponseDto
{
    public Guid? ProductId { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? Name { get; set; }
    public string? ImageUrl { get; set; }
    public decimal? Price { get; set; }
}
