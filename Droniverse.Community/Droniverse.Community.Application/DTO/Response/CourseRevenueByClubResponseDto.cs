namespace Droniverse.Community.Application.DTO.Response;

public class CourseRevenueByClubResponseDto
{
    public Guid ClubId { get; set; }
    public string ClubNameVN { get; set; } = string.Empty;
    public string ClubNameEN { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int TotalLearners { get; set; }
    public decimal TotalRevenue { get; set; }
}
