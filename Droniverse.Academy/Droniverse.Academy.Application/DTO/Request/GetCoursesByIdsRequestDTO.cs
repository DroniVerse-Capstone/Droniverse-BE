using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy;

namespace Droniverse.Academy.Application.DTO.Request;

public class GetCoursesByIdsRequestDTO
{
    public List<Guid> CourseIds { get; set; } = [];
}
