using System;

namespace Droniverse.Shared.DTOs.Response
{
    public class CourseStatisticInterServiceDto
    {
        public Guid CourseId { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public string TitleVN { get; set; } = string.Empty;
        public string TitleEN { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int TotalLearners { get; set; }
        public decimal AverageRating { get; set; }
    }
}
