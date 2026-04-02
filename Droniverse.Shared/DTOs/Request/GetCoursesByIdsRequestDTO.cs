using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy;

namespace Droniverse.Shared.DTOs.Request;

public class GetCoursesByIdsRequestDTO
{
    public List<Guid> CourseIds { get; set; } = [];
}
