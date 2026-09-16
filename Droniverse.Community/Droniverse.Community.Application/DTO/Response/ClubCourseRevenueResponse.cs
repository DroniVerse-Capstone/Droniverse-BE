using Droniverse.Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Community.Application.DTO.Response
{
    public record ClubCourseRevenueResponse
    {
        public required List<CourseRevenueStat> RevenueByCourse { get; set; }
    }

    public record CourseRevenueStat
    {
        public required SimpleCourseResponse CourseInfo { get; set; }
        public decimal Revenue { get; set; }
    }
}
