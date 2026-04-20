using Droniverse.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public class ManagerCourseBulkSearchRequest : SearchRequest
    {
        public ClubCourseProfit? ProfitType { get; set; }
        public ManagerCourseSortBy? CourseSortBy { get; set; } = ManagerCourseSortBy.Participants_Quantity;
        public SortDirection? CourseSortDirection { get; set; } = SortDirection.Asc;
    }

    public enum ManagerCourseSortBy
    {
        Total_Codes_Quantity,
        Remaining_Codes_Quantity,
        Participants_Quantity
    }
}
