using Droniverse.Academy.Domain.Enums;
using Droniverse.Shared.DTOs;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseOverviewResponseDTO
{
    public SimpleUserReponse? Author { get; set; }
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
    public SimpleUserReponse? LastUpdatedBy { get; set; }
    public DateTime? LastUpdatedAt { get; set; }

    public ProductMiniResponseDTO? MiniProduct { get; set; }
}
