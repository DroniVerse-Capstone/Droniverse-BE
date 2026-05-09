using Droniverse.Shared.DTOs.Response;

namespace Droniverse.Community.Application.DTO.Response
{
    public class CourseDetailDashboardResponseDto : CourseStatisticInterServiceDto
    {
        public decimal TotalRevenue { get; set; }
    }
}
