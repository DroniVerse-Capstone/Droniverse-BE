using Droniverse.Community.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Request
{
    public class AddClubCourseRequest
    {
        public int TotalQuanitty { get; set; }
        public int UsedQuantity { get; set; }
        public ClubCourseProfit IsProfit { get; set; }
    }
}
