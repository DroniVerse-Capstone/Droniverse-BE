using Droniverse.Academy.Application.Enums;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Request
{
    public class CourseBulkSearchRequest
    {
        public CourseLevel? Level { get; set; }
        public CourseParticipationFilter? NumberOfParticipation { get; set; }
        public CourseOwnerFilter CourseOwner { get; set; } = CourseOwnerFilter.All;
        public string? CourseName { get; set; }
    }
}
