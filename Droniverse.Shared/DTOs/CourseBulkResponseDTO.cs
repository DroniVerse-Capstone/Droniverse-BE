using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.Enums.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Droniverse.Shared.DTOs.Response
{
    public record CourseBulkResponseDTO
    {
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public required string TitleVN { get; set; }
        public required string TitleEN { get; set; }
        public CourseLevel Level { get; set; }
        public int? EstimatedDuration { get; set; }
        public decimal? Price { get; set; }
        public int RemainingCode { get; set; }
        public decimal Rating { get; set; }
        public int NumberOfParticipants { get; set; }
        public string? ImageUrl { get; set; }
    }
}
