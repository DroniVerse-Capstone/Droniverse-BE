using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.QueryModels
{
    public class CourseEnrollmentQueryModel
    {
        public Guid EnrollmentId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseNameVN { get; set; } = string.Empty;
        public string CourseNameEN { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public CourseLevel Level { get; set; }
        public int? EstimatedDuration { get; set; }
        public float Progress { get; set; }
        public EnrollStatus EnrollStatus { get; set; }
    }
}
