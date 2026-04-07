using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public record ClubCourseResponseDto
    {
        public Guid ClubId { get; init; }
        public Guid CourseId { get; init; }
        public int TotalQuantity { get; init; }
        public int RemainingQuantity { get; init; }
        public ClubCourseProfit ProfitType { get; init; }
        public bool IsAvailable { get; init; }
    }
}
