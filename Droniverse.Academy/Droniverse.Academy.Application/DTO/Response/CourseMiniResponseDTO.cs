using System;

namespace Droniverse.Academy.Application.DTO.Response;

public class CourseMiniResponseDTO
{
    public Guid CourseID { get; set; }

    public LevelMiniResponse? Level { get; set; }

    public CourseVersionMiniResponseDTO? CurrentVersion { get; set; }
}