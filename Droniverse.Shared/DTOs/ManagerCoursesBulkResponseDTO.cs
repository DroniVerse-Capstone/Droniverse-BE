using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs.Response;
using Droniverse.Shared.Enums;

namespace Droniverse.Shared.DTOs
{
    public record ManagerCoursesBulkResponseDTO
    {
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public required string ImageUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int? EstimatedDuration { get; set; }
        public int NumberOfParticipants { get; set; }
        public ManagerClubCourseOwnedResponse? ClubCourseInfo { get; set; }
        public decimal? Price { get; set; }
    }

    public record ManagerClubCourseOwnedResponse
    {
        public int TotalCode { get; set; }
        public int RemainingCode { get; set; }
        public ClubCourseProfit ProfitType { get; set; }
    }
}
