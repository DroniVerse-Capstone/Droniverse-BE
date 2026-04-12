using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.QueryModels
{
    public class CourseInfoQueryModel
    {
        public Guid CourseId { get; set; }
        public required string CourseNameVN { get; set; }
        public required string CourseNameEN { get; set; }
        public CourseStatus CourseStatus { get; set; }
    }
}
