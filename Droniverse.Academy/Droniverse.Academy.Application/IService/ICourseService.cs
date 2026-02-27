using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Domain.Enums;

namespace Droniverse.Academy.Application.IService;

public interface ICourseService
{
    Task<IEnumerable<CourseVersionResponseDto>> GetAllCourses();

    Task<CourseVersionResponseDto> GetCourseById(Guid courseId);

    Task<CourseVersionResponseDto> CreateCourse();

    Task PublishCourse(Guid courseId);
    Task UnpublishCourse(Guid courseId);

    Task DeleteCourse(Guid courseId);
}

