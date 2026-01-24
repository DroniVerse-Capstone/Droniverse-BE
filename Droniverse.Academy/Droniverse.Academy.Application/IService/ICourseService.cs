using Droniverse.Academy.Application.DTO.Response;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{
    Task<IEnumerable<CourseVersionResponseDto>> GetAllCourses();
}

