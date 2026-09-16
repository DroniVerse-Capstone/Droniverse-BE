namespace Droniverse.Community.Application.DTO.Response;

public class DetailDashboardUserResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public decimal TotalSpent { get; set; }
}