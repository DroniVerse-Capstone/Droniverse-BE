namespace Droniverse.Shared.DTOs.Request;

public class GetCoursesByIdsRequestDTO
{
    public List<Guid> CourseIds { get; set; } = [];
}
