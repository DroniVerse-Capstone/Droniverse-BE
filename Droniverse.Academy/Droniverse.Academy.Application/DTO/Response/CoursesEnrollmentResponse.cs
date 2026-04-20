using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response
{
    public record CoursesEnrollmentResponse
    {
        public Guid EnrollmentId { get; set; }
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public required string CourseNameVN { get; set; }
        public required string CourseNameEN { get; set; }
        public string? ImageUrl { get; set; }
        public int? EstimatedDuration { get; set; }
        public float Progress { get; set; }
        public EnrollStatus EnrollStatus { get; set; } = EnrollStatus.ACTIVE;
    }
}
