using Droniverse.Shared.Enums;
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
        public CourseLevelMiniResponseDTO? Level { get; set; }
        public CourseDroneMiniResponseDTO? Drone { get; set; }
        public int NumberOfParticipants { get; set; }
        public decimal Rating { get; set; }
        public string? ImageUrl { get; set; }
        public int? EstimatedDuration { get; set; }
        public decimal? Price { get; set; }
    }

    public record CourseLevelMiniResponseDTO
    {
        public Guid LevelID { get; set; }
        public int LevelNumber { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public record CourseDroneMiniResponseDTO
    {
        public Guid DroneID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ImgURL { get; set; } = string.Empty;
    }
}
