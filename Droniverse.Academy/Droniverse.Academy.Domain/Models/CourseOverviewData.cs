using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Domain.Models;

public class CourseOverviewData
{
    public Guid AuthorId { get; set; }
    public Guid CourseID { get; set; }
    public Guid CourseVersionID { get; set; }
    public string TitleVN { get; set; } = null!;
    public string TitleEN { get; set; } = null!;
    public string? DescriptionVN { get; set; }
    public string? DescriptionEN { get; set; }
    public string? ContextVN { get; set; }
    public string? ContextEN { get; set; }
    public string? ImageUrl { get; set; }
    public CourseLevel Level { get; set; }
    public int? EstimatedDuration { get; set; }

    public decimal AverageRating { get; set; }
    public int TotalFeedback { get; set; }

    public int TotalLearners { get; set; }

    public int TotalModules { get; set; }
    public int TotalTheory { get; set; }
    public int TotalQuiz { get; set; }
    public int TotalLab { get; set; }

    public string? CertificateImageUrl { get; set; }
    public bool IsUnlock { get; set; }
    public Guid? LastUpdatedById { get; set; }
    public DateTime? LastUpdatedAt { get; set; }
}
