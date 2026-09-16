using Droniverse.Shared.Enums;

namespace Droniverse.Shared.DTOs.Response;

public record ClubCourseRemainingQuantityResponseDto
{
    public int RemainingQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public ClubCourseProfit ProfitType { get; set; }
}
