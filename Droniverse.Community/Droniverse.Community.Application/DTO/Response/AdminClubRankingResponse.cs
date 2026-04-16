using Droniverse.Shared.DTOs;

namespace Droniverse.Community.Application.DTO.Response
{
    public record AdminClubRankingResponse
    {
        public required List<ClubRankingItem> Clubs { get; set; }
    }

    public class ClubRankingItem
    {
        public Guid ClubID { get; set; }
        public string NameVN { get; set; }
        public string NameEN { get; set; }
        public string? ImageUrl { get; set; }
        public string ClubCode { get; set; }
        public decimal TotalSpent { get; set; } // Tổng tiền bỏ ra (CLUB_IMPORT)
        public int TransactionCount { get; set; } // Số lần mua
        public List<SimpleCourseResponse> Courses { get; set; } = new(); // Các khóa học đã mua
    }
}
