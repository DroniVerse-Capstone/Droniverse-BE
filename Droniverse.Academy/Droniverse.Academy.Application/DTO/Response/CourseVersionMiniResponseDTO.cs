using System;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseVersionMiniResponseDTO
{
    public Guid CourseVersionID { get; set; }

    public string TitleVN { get; set; } = string.Empty;

    public string TitleEN { get; set; } = string.Empty;

    public int Version { get; set; }
}