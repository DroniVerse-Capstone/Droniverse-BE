using Droniverse.Community.Domain.Enums;

namespace Droniverse.Community.Application.DTO.Request
{
    public class UpdateClubCourseRequest
    {
        public int? TotalQuantity { get; set; }
        public ClubCourseProfit? ProfitType { get; set; }
    }
}
