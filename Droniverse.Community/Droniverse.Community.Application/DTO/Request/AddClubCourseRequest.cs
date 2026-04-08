using Droniverse.Community.Domain.Enums;
using Droniverse.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public class AddClubCourseRequest
    {
        public Guid CourseId { get; set; }
        public int TotalQuantity { get; set; }
        public ClubCourseProfit ProfitType { get; set; } = ClubCourseProfit.PROFIT;
    }
}
