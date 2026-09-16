using System;
using System.Collections.Generic;

namespace Droniverse.Academy.Application.DTO.Response;

public class LevelPathResponseDTO
{
    public LevelMiniResponse Level { get; set; } = null!;

    public List<CourseMiniResponseDTO> Courses { get; set; } = [];
}