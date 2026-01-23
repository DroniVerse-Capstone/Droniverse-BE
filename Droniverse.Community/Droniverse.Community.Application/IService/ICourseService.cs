using Droniverse.Community.Application.DTO.Response;
using Droniverse.Community.Domain.Entities;

namespace Droniverse.Community.Application.IService;
public interface ICourseService
{
    Task<IEnumerable<CourseResponse>> GetAllCourses();
}

