using Droniverse.Community.Domain.Enums;

namespace Droniverse.Shared.DTOs
{
    public record SimpleClubResponse
    {
        public Guid ClubId { get; set; }
        public required string ClubNameVN { get; set; }
        public required string ClubNameEN { get; set; }
        public required string ImageUrl { get; set; }
        public ClubStatus ClubStatus { get; set; }
    }
}
