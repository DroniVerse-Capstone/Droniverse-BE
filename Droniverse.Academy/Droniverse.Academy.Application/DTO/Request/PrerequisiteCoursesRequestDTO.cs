using System;
using System.Collections.Generic;

namespace Droniverse.Academy.Application.DTO.Request;

public class PrerequisiteCoursesRequestDTO
{
    /// <summary>
    /// Danh sách courseId của các khóa học tiền đề.
    /// </summary>
    public IEnumerable<Guid>? PrerequisiteCourseIds { get; set; }
}
