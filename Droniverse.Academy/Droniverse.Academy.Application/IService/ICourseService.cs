using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{
    // List courses
    Task<IEnumerable<CourseVersionResponseDto>> GetAllCourses();

    // Get course detail
    Task<CourseVersionResponseDto> GetCourseById(Guid courseId);

    // Create new course
    Task<CourseVersionResponseDto> CreateCourse(CreateCourseRequestDto request);

    // Publish / Unpublish course
    Task PublishCourse(Guid courseId);
    Task UnpublishCourse(Guid courseId);

    // Delete course
    Task DeleteCourse(Guid courseId);
}

