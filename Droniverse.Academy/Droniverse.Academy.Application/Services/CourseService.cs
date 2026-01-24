using Droniverse.Academy.Application.DTO.Response;
using Droniverse.Academy.Application.IService;
using Droniverse.Academy.Domain.IRepository;

namespace Droniverse.Academy.Application.Services;

internal class CourseService : ICourseService
{
    private readonly IUnitOfWork _unitOfWork;
    public CourseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    public Task<IEnumerable<CourseVersionResponseDto>> GetAllCourses()
    {
        return null;
    }
}
