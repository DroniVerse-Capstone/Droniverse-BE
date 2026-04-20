namespace Droniverse.Community.Application.DTO.Response;

public class MediaResponseDto
{
    public Guid MediaID { get; set; }
    public Guid MediaTypeID { get; set; }
    public string MediaType { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
