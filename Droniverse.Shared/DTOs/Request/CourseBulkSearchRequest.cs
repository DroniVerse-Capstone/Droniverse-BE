using Droniverse.Academy.Application.Enums;

namespace Droniverse.Shared.DTOs.Request
{
    public class CourseBulkSearchRequest : SearchRequest
    {
        public CourseParticipationSort? ParticipationSort { get; set; }
        public string? CourseName { get; set; }
        public Guid? DroneId { get; set; }
        public Guid? LevelId { get; set; }
    }
}
