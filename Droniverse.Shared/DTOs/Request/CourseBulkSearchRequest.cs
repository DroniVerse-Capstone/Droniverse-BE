using Droniverse.Academy.Application.Enums;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Shared.DTOs.Request
{
    public class CourseBulkSearchRequest : SearchRequest
    {
        public CourseLevel? Level { get; set; }
        public CourseParticipationSort? ParticipationSort { get; set; }
        public CourseOwnerFilter CourseOwner { get; set; } = CourseOwnerFilter.All;
        public string? CourseName { get; set; }
    }
}
