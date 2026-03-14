using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.DTO.Response;

public record CourseVersionResponseDTO(
    Guid CourseVersionID,
    string TitleVN,
    string TitleEN,
    string? DescriptionVN,
    string? DescriptionEN,
    CourseVersionStatus Status,
    int Version,
    string? ImageUrl,
    CourseLevel Level,
    int? EstimatedDuration,
    Guid? UpdateBy,
    DateTime? UpdateAt,
    string? ContextVN,
    string? ContextEN,
    IEnumerable<CategoryResponseDTO> Categories,
    IEnumerable<RequiredDroneResponseDTO> RequiredDrones
    );

