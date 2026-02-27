using Droniverse.Academy.Application.DTO.Request;
using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.Enums;
using Droniverse.Academy.Domain.IRepository;
using Droniverse.Shared.Abstractions;

namespace Droniverse.Academy.Application.Services;

public class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    public CourseService(IUnitOfWork unitOfWork, IClock clock, ICurrentUser current)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _currentUser = current;
    }

    public Task<CourseVersionResponseDto> CreateCourse()
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

}
