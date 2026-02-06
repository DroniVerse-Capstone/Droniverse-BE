using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

internal class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<CourseVersionResponseDto> CreateCourse(CreateCourseRequestDto request)
    {
        throw new NotImplementedException();
    }

    public Task DeleteCourse(Guid courseId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<CourseVersionResponseDto>> GetAllCourses()
    {
        return null;
    }

    public Task<CourseVersionResponseDto> GetCourseById(Guid courseId)
    {
        throw new NotImplementedException();
    }

    public Task PublishCourse(Guid courseId)
    {
        throw new NotImplementedException();
    }

    public Task UnpublishCourse(Guid courseId)
    {
        throw new NotImplementedException();
    }

    public Task<CourseVersionResponseDto> UpdateCourseStatus(Guid courseId, CourseStatus status)
    {
        throw new NotImplementedException();
    }
}
