using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Infrastructure.Persistence.MySql.ReadModels;

public class CourseStatsView
{
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string TitleVN { get; set; } = string.Empty;
    public string TitleEN { get; set; } = string.Empty;
    public int? EstimatedDuration { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? UpdateAt { get; set; }
    public int ParticipantCount { get; set; }
    public decimal? AverageRating { get; set; }
}
