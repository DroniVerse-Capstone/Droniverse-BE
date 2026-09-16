namespace Droniverse.Community.Application.DTO.Response;

public class DetailDashboardClubManagerResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public decimal WalletBalance { get; set; }
    public string? ClubNameVN { get; set; }
    public string? ClubNameEN { get; set; }
}
