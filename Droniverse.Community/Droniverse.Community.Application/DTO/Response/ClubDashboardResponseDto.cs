using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubDashboardResponseDto
    {
        public Guid ClubId { get; set; }
        public string ClubNameVN { get; set; } = string.Empty;
        public string ClubNameEN { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalMembers { get; set; }
        public decimal TotalRevenue { get; set; }
        public ClubStatus Status { get; set; }
    }
}
