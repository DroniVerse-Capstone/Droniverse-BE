using System;
using System.Collections.Generic;

namespace Droniverse.Academy.Application.DTO.Request;

public class LevelCoursesRequestDTO
{
    public IEnumerable<Guid>? CourseIds { get; set; }
}