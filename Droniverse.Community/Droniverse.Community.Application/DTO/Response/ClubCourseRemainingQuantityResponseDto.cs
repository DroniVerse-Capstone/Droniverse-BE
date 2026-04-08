using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Enums;

namespace Droniverse.Community.Application.DTO.Response
{
    public class ClubCourseRemainingQuantityResponseDto
    {
        public int RemainingQuantity { get; set; }
        public ClubCourseProfit ProfitType { get; private set; }

    }
}
