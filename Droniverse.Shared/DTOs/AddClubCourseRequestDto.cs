using Droniverse.Shared.Enums;

namespace Droniverse.Shared.DTOs;

public class AddClubCourseRequestDto
{
    public Guid CourseId { get; set; }
    public int TotalQuantity { get; set; }
    public ClubCourseProfit ProfitType { get; set; } = ClubCourseProfit.PROFIT;
}

